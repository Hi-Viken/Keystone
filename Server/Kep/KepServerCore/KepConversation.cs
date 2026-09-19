using System.Collections.Concurrent;
using System.Net.Security;
using System.Net.Sockets;
using KepCommon;
using ZepCommon;

namespace KepServerCore
{
    public class KepConversation
    {
        /// <summary>会话 ID</summary>
        public string Id { get; }

        /// <summary>客户端连接</summary>
        public TcpClient Client { get; }

        /// <summary>加密数据流</summary>
        public SslStream SslStream { get; }

        public CancellationToken ct { get; set; }

        /// <summary>连接停止信号</summary>
        public CancellationTokenSource ConnectCts { get; }

        /// <summary>是否已认证</summary>
        public bool Authenticated { get; set; }

        /// <summary>最后活动时间</summary>
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;

        /// <summary>设备 SN</summary>
        public string? SN { get; set; }

        /// <summary>设备Token/// </summary>
        public string? Token { get; set; }

        /// <summary>写锁（保护并发写入 SslStream）</summary>
        private readonly SemaphoreSlim _writeLock = new(1, 1);

        /// <summary>环形缓冲区（处理粘包/半包）</summary>
        private readonly RingBuffer _ring = new(8192);

        /// <summary>等待响应的请求表（Id → TCS）</summary>
        private readonly ConcurrentDictionary<uint, TaskCompletionSource<DataPack>> _pendingResponses = new();

        /// <summary>自增请求 ID</summary>
        private uint _nextRequestId;

        public KepConversation(string id, TcpClient client, SslStream sslStream, CancellationTokenSource cts)
        {
            Id = id;
            Client = client;
            SslStream = sslStream;
            ConnectCts = cts;
        }

        /// <summary>发送帧（线程安全）</summary>
        public Task SendFrameAsync(DataPack frame, CancellationToken ct = default)
            => DataPack.SendAsync(SslStream, frame, _writeLock, ct);

        /// <summary>接收帧（基于 RingBuffer，处理粘包/半包）</summary>
        public Task<DataPack> ReceiveFrameAsync(CancellationToken ct = default)
            => DataPack.ReceiveAsync(SslStream, _ring, ct);

        /// <summary>
        /// 下发参数并等待客户端回复（请求-响应模式）。
        /// 发送 ParamRequest 帧，阻塞直到收到对应 Id 的 ParamResponse 帧。
        /// </summary>
        public async Task<DataPack> SendCommandAsync(byte[] paramData, int timeoutMs = 10_000, CancellationToken ct = default)
        {
            uint reqId = GenerateRequestId();
            var tcs = new TaskCompletionSource<DataPack>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingResponses[reqId] = tcs;

            try
            {
                var request = new DataPack
                {
                    FunctionCode = ProtocolConstants.ParamRequest,
                    Id = reqId,
                    Data = paramData
                };
                await SendFrameAsync(request, ct);

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(timeoutMs);
                using var reg = timeoutCts.Token.Register(() => tcs.TrySetException(new TimeoutException($"等待响应超时 ({timeoutMs}ms)")));
                var resp= await tcs.Task;
                return resp;
            }
            finally
            {
                _pendingResponses.TryRemove(reqId, out _);
            }
        }

        /// <summary>尝试完成一个等待中的响应（由消息循环调用）</summary>
        public bool TryCompleteResponse(DataPack response)
        {
            if (_pendingResponses.TryRemove(response.Id, out var tcs))
            {
                tcs.TrySetResult(response);
                return true;
            }
            return false;
        }

        /// <summary>纯文本阶段：读取一行（以 \r\n 结束）</summary>
        public async Task<string> ReadTextLineAsync(CancellationToken ct = default)
        {
            var sb = new System.Text.StringBuilder();
            var buf = new byte[1];
            bool prevCr = false;
            while (true)
            {
                int read = await SslStream.ReadAsync(buf, ct);
                if (read == 0) throw new EndOfStreamException("连接已断开");
                char ch = (char)buf[0];
                if (ch == '\r') { prevCr = true; continue; }
                if (ch == '\n' && prevCr) break;
                sb.Append(ch);
                prevCr = false;
            }
            return sb.ToString();
        }


        /// <summary>
        /// 生成线程安全自增的不重复请求 ID
        /// </summary>
        private uint GenerateRequestId()
        {
            return Interlocked.Increment(ref _nextRequestId);
        }
        /// <summary>纯文本阶段：发送文本</summary>
        public ValueTask WriteTextAsync(string text, CancellationToken ct = default)
            => SslStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(text), ct);
    }
}
