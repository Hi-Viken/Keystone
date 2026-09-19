using KepCommon;
using System.Collections.Concurrent;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.ConstrainedExecution;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using ZepCommon;

namespace KepClientCore
{
    public class KepClient
    {
        private string? _host;
        private int _port;
        private string? _Sn;
        private string? _Token;
        private TcpClient _client = new();
        private SslStream? _sslStream;
        private readonly SemaphoreSlim _writeLock = new(1, 1);
        private readonly RingBuffer _ring = new(8192);
        private CancellationTokenSource? _cts;
        private readonly PartialFileTracker _downloadTracker = new();
        private readonly ConcurrentDictionary<uint, int> _downloadLastProgress = new();
        private readonly ConcurrentDictionary<uint, int> _uploadLastProgress = new();

        /// <summary>等待响应的请求表（Id → TCS）</summary>
        private readonly ConcurrentDictionary<uint, TaskCompletionSource<DataPack>> _pendingResponses = new();

        /// <summary>自增请求 ID</summary>
        private uint _nextRequestId;

        /// <summary>日志输出</summary>
        public event Action<string>? OnLog;

        /// <summary>认证状态</summary>
        public bool IsAuthenticated { get; private set; }

        /// <summary>帧接收事件（供 UI 层订阅）</summary>
        public event Action<DataPack>? OnFrameReceived;

        /// <summary>收到服务端下发参数事件（供 UI 层订阅）</summary>
        public event Action<string>? OnParamReceived;

        /// <summary>断开连接时</summary>
        public event Action? OnDisconnect;

        public async Task ConnectAsync(string host, int port,string sn,string Toklen)
        {
            _host= host;
            _port= port;
            _Sn= sn;
            _Token= Toklen;
            _cts = new CancellationTokenSource();
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(host, port);
                _client.NoDelay = true;
                var netStream = _client.GetStream();
                // TLS 握手
                _sslStream = new SslStream(netStream, false, ValidateServerCertificate);
                await _sslStream.AuthenticateAsClientAsync(
                    targetHost: "localhost",
                    clientCertificates: null,
                    enabledSslProtocols: SslProtocols.Tls13,
                    checkCertificateRevocation: false);

                Log("TLS 连接成功");

                // 纯文本协商
                await NegotiateVersionAsync(_cts.Token);

                // 身份认证
                bool authOk = await AuthenticateAsync(_Sn, _Token, _cts.Token);
                if (!authOk)
                {
                    Log("认证失败，断开连接");
                    OnDisconnect?.Invoke();
                    return;
                }

                IsAuthenticated = true;

                // 启动后台接收循环
                _ = Task.Run(() => ReadLoopAsync(_cts.Token));

                // 启动后台心跳
                _ = Task.Run(() => HeartbeatLoopAsync(_cts.Token));
            }
            catch (Exception ex)
            {
                Log($"连接错误: {ex.Message}");
            }
        }

        /// <summary>断开连接</summary>
        public void Disconnect()
        {
            _cts?.Cancel();
            _client.Close();
            IsAuthenticated = false;
            Log("已断开连接");
        }

        /// <summary>纯文本协商：接收版本列表 + 选择版本</summary>
        private async Task NegotiateVersionAsync(CancellationToken ct)
        {
            // 读取版本列表（直到 \r\n\r\n）
            var sb = new StringBuilder();
            var buf = new byte[1];
            while (true)
            {
                int read = await _sslStream!.ReadAsync(buf, ct);
                if (read == 0) throw new EndOfStreamException("连接已断开");
                sb.Append((char)buf[0]);
                if (sb.Length >= 4 &&
                    sb.ToString(sb.Length - 4, 4) == "\r\n\r\n")
                    break;
            }

            string versionText = sb.ToString(0, sb.Length - 4);
            string[] versions = versionText.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

            foreach (var v in versions)
                Log("可用版本: " + v);

            // 选择最后一个版本
            string selected = versions[^1];
            Log("选择版本: " + selected);
            byte[] send = Encoding.UTF8.GetBytes(selected + "\r\n");
            await _sslStream.WriteAsync(send, ct);
            await _sslStream.FlushAsync(ct);
        }

        /// <summary>身份认证（帧协议）</summary>
        private async Task<bool> AuthenticateAsync(string sn, string token, CancellationToken ct)
        {
            var identity = new Identity(sn, token);
            var request = new DataPack
            {
                FunctionCode = ProtocolConstants.AuthRequest,
                Id = 1,
                Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(identity))
            };
            await DataPack.SendAsync(_sslStream!, request, _writeLock, ct);
            Log($"发送认证: SN={sn}");

            // 读取认证响应
            var response = await DataPack.ReceiveAsync(_sslStream, _ring, ct);
            var result = JsonSerializer.Deserialize<AuthResult>(response.Data);
            Log($"认证结果: {(result?.Success == true ? "成功" : "失败")} - {result?.Message}");

            return result?.Success == true;
        }

        /// <summary>后台帧接收循环</summary>
        private async Task ReadLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var frame = await DataPack.ReceiveAsync(_sslStream!, _ring, ct);

                    switch (frame.FunctionCode)
                    {
                        case ProtocolConstants.HeartbeatPong:
                            string pongTs = frame.GetStringData();
                            if (long.TryParse(pongTs, out long sentTs))
                            {
                                long rtt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - sentTs;
                                Log($"心跳响应 RTT={rtt}ms");
                            }
                            break;

                        case ProtocolConstants.TimeSyncResponse:
                            string serverTs = frame.GetStringData();
                            if (long.TryParse(serverTs, out long ts))
                            {
                                var serverTime = DateTimeOffset.FromUnixTimeMilliseconds(ts);
                                long localTs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                Log($"服务器时间: {serverTime:yyyy-MM-dd HH:mm:ss.fff} 偏差: {ts - localTs}ms");
                            }
                            break;

                        case ProtocolConstants.TempAck:
                            Log($"温度确认: {frame.GetStringData()}");
                            break;

                        case ProtocolConstants.ParamRequest:
                            string param = frame.GetStringData();
                            Log($"收到服务端下发参数: {param}");
                            OnParamReceived?.Invoke(param);
                            var paramResp = new DataPack
                            {
                                FunctionCode = ProtocolConstants.ParamResponse,
                                Id = frame.Id,
                                Data = Encoding.UTF8.GetBytes($"ACK:{param}")
                            };
                            await DataPack.SendAsync(_sslStream!, paramResp, _writeLock, ct);
                            Log($"已回复参数确认");
                            break;

                        case ProtocolConstants.ClientResponse:
                            if (TryCompleteResponse(frame))
                                Log($"收到服务端响应: {frame.GetStringData()}");
                            else
                                Log($"收到未匹配的 ClientResponse Id={frame.Id}");
                            break;

                        case ProtocolConstants.FileDownload:
                            HandleFileDownload(frame);
                            break;

                        case ProtocolConstants.FileUploadAck:
                            Log($"文件上传确认: {frame.GetStringData()}");
                            break;

                        default:
                            Log($"收到帧: {frame}");
                            break;
                    }

                    OnFrameReceived?.Invoke(frame);
                }
            }
            catch (OperationCanceledException) { }
            catch (EndOfStreamException)
            {
                Log("服务器断开连接");
                IsAuthenticated = false;
                OnDisconnect?.Invoke();
            }
            catch (IOException ioEx) when (ioEx.InnerException is SocketException se && se.SocketErrorCode == SocketError.ConnectionReset)
            {

                Log("服务端强制断开RST，执行重连");
                IsAuthenticated = false;
                OnDisconnect?.Invoke();
            }
            catch (IOException)
            {
                Log("其他IO异常");
                IsAuthenticated = false;
                OnDisconnect?.Invoke();
                // 其他IO异常
            }
            catch (Exception ex)
            {
                Log($"接收异常: {ex.Message}");
                IsAuthenticated = false;
                OnDisconnect?.Invoke();
            }
        }

        /// <summary>后台心跳循环（10s 间隔）</summary>
        private async Task HeartbeatLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && IsAuthenticated)
                {
                    await Task.Delay(10_000, ct);
                    if (ct.IsCancellationRequested || !IsAuthenticated) break;
                    await SendHeartbeatAsync(ct);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { Log($"心跳异常: {ex.Message}"); }
        }

        /// <summary>发送心跳</summary>
        public async Task SendHeartbeatAsync(CancellationToken ct = default)
        {
            long ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var frame = new DataPack
            {
                FunctionCode = ProtocolConstants.HeartbeatPing,
                Data = Encoding.UTF8.GetBytes(ts.ToString())
            };
            await DataPack.SendAsync(_sslStream!, frame, _writeLock, ct);
            Log("心跳已发送");
        }

        /// <summary>发送对时请求</summary>
        public async Task SendTimeSyncAsync(CancellationToken ct = default)
        {
            long ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var frame = new DataPack
            {
                FunctionCode = ProtocolConstants.TimeSyncRequest,
                Data = Encoding.UTF8.GetBytes(ts.ToString())
            };
            await DataPack.SendAsync(_sslStream!, frame, _writeLock, ct);
            Log("对时请求已发送");
        }

        /// <summary>上报温度</summary>
        public async Task SendTemperatureAsync(string temperature, CancellationToken ct = default)
        {
            var frame = new DataPack
            {
                FunctionCode = ProtocolConstants.TempUpload,
                Data = Encoding.UTF8.GetBytes(temperature)
            };
            await DataPack.SendAsync(_sslStream!, frame, _writeLock, ct);
            Log($"温度上报: {temperature}");
        }

        /// <summary>
        /// 上报数据并等待服务端回复（请求-响应模式）。
        /// 发送 ClientRequest 帧，阻塞直到收到对应 Id 的 ClientResponse 帧。
        /// </summary>
        public async Task<DataPack> SendRequestAndWaitAsync(byte[] requestData, int timeoutMs = 10_000, CancellationToken ct = default)
        {
            uint reqId = GenerateRequestId();
            var tcs = new TaskCompletionSource<DataPack>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingResponses[reqId] = tcs;

            try
            {
                var request = new DataPack
                {
                    FunctionCode = ProtocolConstants.ClientRequest,
                    Id = reqId,
                    Data = requestData
                };
                await DataPack.SendAsync(_sslStream!, request, _writeLock, ct);

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(timeoutMs);
                using var reg = timeoutCts.Token.Register(() => tcs.TrySetException(new TimeoutException($"等待响应超时 ({timeoutMs}ms)")));
                var resp = await tcs.Task;
                return resp;
            }
            finally
            {
                _pendingResponses.TryRemove(reqId, out _);
            }
        }

        /// <summary>尝试完成一个等待中的响应（由消息循环调用）</summary>
        private bool TryCompleteResponse(DataPack response)
        {
            if (_pendingResponses.TryRemove(response.Id, out var tcs))
            {
                tcs.TrySetResult(response);
                return true;
            }
            return false;
        }

        /// <summary>处理服务端下发的文件（分片接收）</summary>
        private void HandleFileDownload(DataPack frame)
        {
            try
            {
                bool completed = _downloadTracker.HandleChunk(frame, out string? fileName, out byte[]? fileData,
                    out long receivedBytes, out long totalSize);

                // 每10%打印一次进度
                if (totalSize > 0)
                {
                    int progressPercent = (int)((receivedBytes * 100) / totalSize);
                    int lastPercent = _downloadLastProgress.GetValueOrDefault(frame.Id, -1);
                    if (progressPercent / 10 > lastPercent / 10)
                    {
                        _downloadLastProgress[frame.Id] = progressPercent;
                        Log($"下发文件进度: {fileName} {progressPercent}% ({receivedBytes}/{totalSize} 字节)");
                    }
                }

                if (completed && fileName != null && fileData != null)
                {
                    _downloadLastProgress.TryRemove(frame.Id, out _);

                    // 保存到 data/ 目录
                    string dirPath = "data";
                    Directory.CreateDirectory(dirPath);
                    string filePath = Path.Combine(dirPath, fileName);
                    File.WriteAllBytes(filePath, fileData);

                    Log($"下发文件完成: {fileName} ({fileData.Length} 字节) -> {filePath}");

                    // 发送确认
                    var ack = new DataPack
                    {
                        FunctionCode = ProtocolConstants.FileDownloadAck,
                        Id = frame.Id,
                        Data = Encoding.UTF8.GetBytes($"OK:{fileName}")
                    };
                    _ = DataPack.SendAsync(_sslStream!, ack, _writeLock, _cts!.Token);
                }
            }
            catch (Exception ex)
            {
                Log($"处理下发文件异常: {ex.Message}");
            }
        }

        /// <summary>上传文件到服务端（分片发送）</summary>
        public async Task SendFileToServerAsync(string filePath, CancellationToken ct = default)
        {
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
                    FunctionCode = ProtocolConstants.FileUpload,
                    Id = fileId,
                    Data = PartialFileTracker.BuildChunkData(fileName, totalSize, offset, chunkData)
                };
                await DataPack.SendAsync(_sslStream!, frame, _writeLock, ct);
                offset += chunkLen;
                chunkCount++;

                // 每10%打印一次进度
                int progressPercent = (int)((offset * 100) / totalSize);
                if (progressPercent / 10 > lastProgressPercent / 10)
                {
                    lastProgressPercent = progressPercent;
                    Log($"上传文件进度: {fileName} {progressPercent}% ({offset}/{totalSize} 字节)");
                }
            }
            Log($"上传文件完成: {fileName} ({totalSize} 字节, {chunkCount} 片)");
        }

        // 服务器证书指纹（改成你自己的）
        private const string ServerCertThumbprint = "331D5AEBD84247D837E2076C30EC2E5587F297DE";

        private bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            if (certificate == null)
                return false;

            X509Certificate2 cert2 = new X509Certificate2(certificate);


            Log("===== 服务端证书信息 =====");
            Log($"Subject(主体): {cert2.Subject}");
            Log($"Issuer(颁发者): {cert2.Issuer}");
            Log($"生效时间NotBefore(UTC): {cert2.NotBefore:yyyy‑MM‑dd HH:mm:ss}");
            Log($"过期时间NotAfter(UTC): {cert2.NotAfter:yyyy‑MM‑dd HH:mm:ss}");
            Log($"序列号SerialNumber: {cert2.SerialNumber}");
            // 兼容全平台获取 SHA256 指纹（替代GetCertHash256()）
            byte[] hashSha256 = cert2.GetCertHash(HashAlgorithmName.SHA256);
            string sha256Finger = BitConverter.ToString(hashSha256).Replace("-", "");
            Log($"证书SHA256指纹: {sha256Finger}");
            //Console.WriteLine($"证书SHA256指纹: {BitConverter.ToString(cert2.GetCertHash256()).Replace("-", "")}");
            Log($"SHA1指纹: {BitConverter.ToString(cert2.GetCertHash()).Replace("-", "")}");




            if (string.Equals(cert2.Thumbprint, ServerCertThumbprint, StringComparison.OrdinalIgnoreCase))
                return true;

            Log($"证书不匹配，拒绝连接，错误：{sslPolicyErrors}");
            return false;
        }


        /// <summary>
        /// 生成线程安全自增的不重复请求 ID
        /// </summary>
        private uint GenerateRequestId()
        {
            return Interlocked.Increment(ref _nextRequestId);
        }

        internal void Log(string message) => OnLog?.Invoke(message);
    }
}
