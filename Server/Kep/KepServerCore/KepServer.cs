using KepCommon;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using ZepCommon;

namespace KepServerCore
{
    public class KepServer
    {
        private TcpListener? _listener;
        public readonly string IpAddress;
        public readonly int Port;
        public DateTime? StartTime;
        private Task? _acceptTask;
        private CancellationTokenSource _serverCts = new();
        private readonly ConcurrentDictionary<string, KepConversation> _onlineClients = new();
        private readonly PartialFileTracker _uploadTracker = new();
        private readonly ConcurrentDictionary<uint, int> _uploadLastProgress = new();
        public event Action<string>? OnLog;

        /// <summary>
        /// 身份认证事件
        /// </summary>
        public event Func<Identity,bool>? OnAuthentication;

        /// <summary>
        /// 客户端上线
        /// </summary>
        public event Action<KepConversation>? OnClientOnline;
        /// <summary>
        /// 客户端下线
        /// </summary>
        public event Action<string>? OnClientOffline;

        

        /// <summary>客户端上报请求事件 (clientId, requestId, requestData)</summary>
        public event Action<string, uint, string>? OnClientRequestReceived;

        /// <summary>版本列表</summary>
        private static readonly string[] Versions = { "v1.0.0", "v1.1.0", "v1.2.0", "v2.0.0-beta", "v2.0.0" };

        public KepServer(string ipAddress, int port)
        {
            IpAddress = ipAddress;
            Port = port;
        }

        public bool Start()
        {
            _listener = new TcpListener(IPAddress.Parse(IpAddress), Port);
            _listener.Start();
            Log($"TCP 服务启动，监听端口：{Port}");
            Log("等待客户端接入...\n");
            _acceptTask = AcceptLoopAsync();
            StartTime= DateTime.Now;
            return true;
        }

        public async Task StopAsync()
        {
            StartTime = null;
            Log("正在停止 TCP 服务...");
            _serverCts.Cancel();
            _listener?.Stop();

            if (_acceptTask != null)
            {
                try { await _acceptTask.WaitAsync(TimeSpan.FromSeconds(2)); }
                catch { }
            }

            foreach (var id in _onlineClients.Keys.ToList())
                Disconnect(id);

            Log("TCP 服务已完全停止\n");
        }

        /// <summary>
        /// 获取在线数量
        /// </summary>
        /// <returns></returns>
        public int GetOnlineCount()
        { 
            return _onlineClients.Count;
        }

        private async Task AcceptLoopAsync()
        {
            try
            {
                while (!_serverCts.Token.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync(_serverCts.Token);
                    client.NoDelay = true;
                    var clientIp = client.Client.RemoteEndPoint ?? (EndPoint)new IPEndPoint(IPAddress.None, 0);
                    var clientId = Guid.NewGuid().ToString("N")[..8];

                    var clientCts = CancellationTokenSource.CreateLinkedTokenSource(_serverCts.Token);

                    // TLS 握手
                    var sslStream = new SslStream(client.GetStream(), leaveInnerStreamOpen: false);
                    await sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
                    {
                        ServerCertificate = CreateTlsCertificate(),
                        ClientCertificateRequired = false,
                        EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                    });
                    Log($"[Server] TLS 握手完成：{clientIp}");

                    var conversation = new KepConversation(clientId, client, sslStream, clientCts);
                    _onlineClients.TryAdd(clientId, conversation);
                    Log($"客户端上线 [{clientId}] | 在线：{_onlineClients.Count}");

                    // 每个客户端独立 Task 处理
                    _ = Task.Run(async () =>
                    {
                        try { await HandleConversationAsync(conversation); }
                        catch (Exception ex) { Log($"会话 [{conversation.Id}] 异常: {ex.GetType().Name}: {ex.Message}"); if (ex.InnerException != null) Log($"  Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}"); }
                        finally
                        {
                            Disconnect(conversation.Id);
                            conversation.Client.Close();
                        }
                    });
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception ex) { Log($"接受连接错误：{ex}"); }
        }

        /// <summary>处理单个客户端会话的完整生命周期</summary>
        private async Task HandleConversationAsync(KepConversation conv)
        {
            var ct = conv.ConnectCts.Token;

            // ===== 阶段 1：纯文本 - 下发版本列表 =====
            string versionList = string.Join("\r\n", Versions) + "\r\n\r\n";
            await conv.WriteTextAsync(versionList, ct);
            await conv.SslStream.FlushAsync(ct);
            Log($"[{conv.Id}] 下发版本列表 ({Versions.Length} 个版本)");

            // ===== 阶段 2：纯文本 - 等待客户端选择版本 =====
            string selectedVersion = await conv.ReadTextLineAsync(ct);
            Log($"[{conv.Id}] 客户端选择版本: {selectedVersion}");

            // ===== 阶段 3：帧协议 - 身份认证 =====
            Log($"[{conv.Id}] 等待认证帧...");
            var authFrame = await conv.ReceiveFrameAsync(ct);
            Log($"[{conv.Id}] 收到认证帧: {authFrame}");
            var authInfo = JsonSerializer.Deserialize<Identity>(authFrame.Data) ?? new Identity();
            conv.SN = authInfo.SN;
            conv.Token = authInfo.Token;
            Log($"[{conv.Id}] 认证信息: SN={authInfo.SN}, Token={authInfo.Token}");



            // 简化写法
            bool ok = OnAuthentication?.Invoke(authInfo) ?? false;

            //bool ok = !string.IsNullOrEmpty(authInfo.SN) && !string.IsNullOrEmpty(authInfo.Token);



            var result = new AuthResult
            {
                Success = ok,
                Message = ok ? "认证成功" : "SN 或 Token 无效"
            };
            var respFrame = new DataPack
            {
                FunctionCode = ProtocolConstants.AuthResponse,
                Id = authFrame.Id,
                Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result))
            };

            await conv.SendFrameAsync(respFrame, ct);
            Log($"[{conv.Id}] 认证结果: {(ok ? "成功" : "失败")}");

            if (!ok) return;

            conv.Authenticated = true;
            conv.LastActivity = DateTime.UtcNow;
            OnClientOnline?.Invoke(conv);

            // ===== 阶段 4：心跳超时监控 =====
            _ = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    await Task.Delay(5000, ct);
                    if (conv.Authenticated && (DateTime.UtcNow - conv.LastActivity).TotalSeconds > 30)
                    {
                        Log($"[{conv.Id}] 心跳超时，断开连接");
                        conv.ConnectCts.Cancel();
                        break;
                    }
                }
            }, ct);

            // ===== 阶段 5：主消息循环 =====
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var frame = await conv.ReceiveFrameAsync(ct);
                    conv.LastActivity = DateTime.UtcNow;

                    switch (frame.FunctionCode)
                    {
                        case ProtocolConstants.HeartbeatPing:
                            var pong = new DataPack
                            {
                                FunctionCode = ProtocolConstants.HeartbeatPong,
                                Id = frame.Id,
                                Data = frame.Data
                            };
                            await conv.SendFrameAsync(pong, ct);
                            Log($"[{conv.Id}] 心跳");
                            break;

                        case ProtocolConstants.TimeSyncRequest:
                            long ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            var timeResp = new DataPack
                            {
                                FunctionCode = ProtocolConstants.TimeSyncResponse,
                                Id = frame.Id,
                                Data = Encoding.UTF8.GetBytes(ts.ToString())
                            };
                            await conv.SendFrameAsync(timeResp, ct);
                            Log($"[{conv.Id}] 对时响应: {ts}");
                            break;

                        case ProtocolConstants.TempUpload:
                            string temp = frame.GetStringData();
                            var tempAck = new DataPack
                            {
                                FunctionCode = ProtocolConstants.TempAck,
                                Id = frame.Id,
                                Data = Encoding.UTF8.GetBytes($"OK:{temp}")
                            };
                            await conv.SendFrameAsync(tempAck, ct);
                            Log($"[{conv.Id}] 温度上报: {temp}");
                            break;

                        case ProtocolConstants.ParamResponse:
                            if (conv.TryCompleteResponse(frame))
                                Log($"[{conv.Id}] 收到参数回复: {frame.GetStringData()}");
                            else
                                Log($"[{conv.Id}] 收到未匹配的 ParamResponse Id={frame.Id}");
                            break;

                        case ProtocolConstants.ClientRequest:
                            string clientReq = frame.GetStringData();
                            Log($"[{conv.Id}] 收到客户端上报请求: {clientReq}");
                            OnClientRequestReceived?.Invoke(conv.Id, frame.Id, clientReq);
                            break;

                        case ProtocolConstants.FileUpload:
                            HandleFileUpload(conv, frame);
                            break;

                        case ProtocolConstants.FileDownloadAck:
                            Log($"[{conv.Id}] 收到文件下发确认: {frame.GetStringData()}");
                            break;

                        default:
                            Log($"[{conv.Id}] 未知功能码: 0x{frame.FunctionCode:X4}");
                            break;
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (EndOfStreamException) { }
            catch (IOException ioEx) when (ioEx.InnerException is SocketException se && se.SocketErrorCode == SocketError.ConnectionReset)
            {

                Log("客户端端强制断开RST");
            }
        }

        /// <summary>获取在线客户端 ID 列表</summary>
        public IReadOnlyList<string> GetOnlineClientIds() => _onlineClients.Keys.ToList();

        /// <summary>踢指定客户端下线</summary>
        /// <param name="clientId">客户端 ID</param>
        /// <param name="reason">踢下线原因（可选）</param>
        /// <returns>是否成功踢下线</returns>
        public bool KickClient(string clientId, string? reason = null)
        {
            if (!_onlineClients.TryGetValue(clientId, out var conv))
            {
                Log($"踢客户端失败：[{clientId}] 不在线");
                return false;
            }

            Log($"正在踢客户端 [{clientId}] 下线" + (string.IsNullOrEmpty(reason) ? "" : $"，原因：{reason}"));
            
            // 取消会话的 CancellationToken，触发正常断开流程
            try
            {
                conv.ConnectCts.Cancel();
            }
            catch (Exception ex)
            {
                Log($"踢客户端 [{clientId}] 时发生异常：{ex.Message}");
            }

            // Disconnect 方法会在 HandleConversationAsync 的 finally 块中被调用
            // 这里直接调用以确保立即清理
            Disconnect(clientId);
            
            return true;
        }

        /// <summary>向指定客户端下发参数并等待回复</summary>
        public async Task<string> SendCommandToClientAsync(string clientId, string paramText, CancellationToken ct = default)
        {
            if (!_onlineClients.TryGetValue(clientId, out var conv))
                throw new InvalidOperationException($"客户端 [{clientId}] 不在线");

            Log($"[{clientId}] 下发参数: {paramText}");
            var response = await conv.SendCommandAsync(Encoding.UTF8.GetBytes(paramText), ct: ct);
            return response.GetStringData();
        }

        /// <summary>手动回复客户端上报请求</summary>
        public async Task ReplyToClientRequestAsync(string clientId, uint requestId, string replyText, CancellationToken ct = default)
        {
            if (!_onlineClients.TryGetValue(clientId, out var conv))
                throw new InvalidOperationException($"客户端 [{clientId}] 不在线");

            var reply = new DataPack
            {
                FunctionCode = ProtocolConstants.ClientResponse,
                Id = requestId,
                Data = Encoding.UTF8.GetBytes(replyText)
            };
            await conv.SendFrameAsync(reply, ct);
            Log($"[{clientId}] 手动回复客户端请求 Id={requestId}: {replyText}");
        }

        /// <summary>向指定客户端下发文件（分片发送）</summary>
        public async Task SendFileToClientAsync(string clientId, string filePath, CancellationToken ct = default)
        {
            if (!_onlineClients.TryGetValue(clientId, out var conv))
                throw new InvalidOperationException($"客户端 [{clientId}] 不在线");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"文件不存在: {filePath}");

            string fileName = Path.GetFileName(filePath);
            byte[] fileContent = await File.ReadAllBytesAsync(filePath, ct);
            long totalSize = fileContent.Length;
            uint fileId = (uint)Environment.TickCount;

            int offset = 0;
            int chunkCount = 0;
            int lastProgressPercent = -1;
            while (offset < totalSize)
            {
                int chunkLen = (int)Math.Min(ProtocolConstants.FileChunkMaxPayload, totalSize - offset);
                byte[] chunkData = new byte[chunkLen];
                Buffer.BlockCopy(fileContent, offset, chunkData, 0, chunkLen);

                var frame = new DataPack
                {
                    FunctionCode = ProtocolConstants.FileDownload,
                    Id = fileId,
                    Data = PartialFileTracker.BuildChunkData(fileName, totalSize, offset, chunkData)
                };
                await conv.SendFrameAsync(frame, ct);
                offset += chunkLen;
                chunkCount++;

                // 每10%打印一次进度
                int progressPercent = (int)((offset * 100) / totalSize);
                if (progressPercent / 10 > lastProgressPercent / 10)
                {
                    lastProgressPercent = progressPercent;
                    Log($"[{clientId}] 下发文件进度: {fileName} {progressPercent}% ({offset}/{totalSize} 字节)");
                }
            }
            Log($"[{clientId}] 下发文件完成: {fileName} ({totalSize} 字节, {chunkCount} 片)");
        }

        /// <summary>处理客户端上传的文件（分片接收）</summary>
        private void HandleFileUpload(KepConversation conv, DataPack frame)
        {
            try
            {
                bool completed = _uploadTracker.HandleChunk(frame, out string? fileName, out byte[]? fileData,
                    out long receivedBytes, out long totalSize);

                // 每10%打印一次进度
                if (totalSize > 0)
                {
                    int progressPercent = (int)((receivedBytes * 100) / totalSize);
                    int lastPercent = _uploadLastProgress.GetValueOrDefault(frame.Id, -1);
                    if (progressPercent / 10 > lastPercent / 10)
                    {
                        _uploadLastProgress[frame.Id] = progressPercent;
                        Log($"[{conv.Id}] 上传文件进度: {fileName} {progressPercent}% ({receivedBytes}/{totalSize} 字节)");
                    }
                }

                if (completed && fileName != null && fileData != null)
                {
                    _uploadLastProgress.TryRemove(frame.Id, out _);

                    // 保存到 data/{clientId}/ 目录
                    string dirPath = Path.Combine("data", conv.Id);
                    Directory.CreateDirectory(dirPath);
                    string filePath = Path.Combine(dirPath, fileName);
                    File.WriteAllBytes(filePath, fileData);

                    Log($"[{conv.Id}] 上传文件完成: {fileName} ({fileData.Length} 字节) -> {filePath}");

                    // 发送确认
                    var ack = new DataPack
                    {
                        FunctionCode = ProtocolConstants.FileUploadAck,
                        Id = frame.Id,
                        Data = Encoding.UTF8.GetBytes($"OK:{fileName}")
                    };
                    _ = conv.SendFrameAsync(ack, conv.ConnectCts.Token);
                }
            }
            catch (Exception ex)
            {
                Log($"[{conv.Id}] 处理上传文件异常: {ex.Message}");
            }
        }

        private X509Certificate2 CreateTlsCertificate()
        {
            const string certPath = "server-tls.pfx";
            const string certPassword = "tls-cert-pwd";

            if (File.Exists(certPath))
            {
                Log("[Server] 加载已有 TLS 证书");
                return X509CertificateLoader.LoadPkcs12FromFile(certPath, certPassword);
            }

            Log("[Server] 生成自签名 TLS 证书...");
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest(
                "CN=TcpSecureDemo Server",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

            var cert = request.CreateSelfSigned(
                DateTimeOffset.Now.AddDays(-1),
                DateTimeOffset.Now.AddYears(1));

            Log($"当前证书指纹 Thumbprint: {cert.Thumbprint}");

            var bytes = cert.Export(X509ContentType.Pfx, certPassword);
            File.WriteAllBytes(certPath, bytes);
            Log($"[Server] TLS 证书已保存：{certPath}");

            return X509CertificateLoader.LoadPkcs12(bytes, certPassword);
        }

        private void Disconnect(string id)
        {
            if (!_onlineClients.TryRemove(id, out var conv))
                return;
            try { conv.Client.Close(); conv.ConnectCts.Cancel() ;} catch { }
            OnClientOffline?.Invoke(conv.SN);
            Log($"客户端下线 [{id}] | 在线：{_onlineClients.Count}\n");
        }

        internal void Log(string message) => OnLog?.Invoke(message);
    }
}
