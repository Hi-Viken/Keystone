using KepCommon;
using System.Buffers.Binary;
using System.Text;

namespace ZepCommon
{
    public class DataPack
    {
        /// <summary>
        /// 帧头魔数
        /// </summary>
        public ushort Magic { get; set; } = ProtocolConstants.FrameHeader;
        /// <summary>
        /// 功能码
        /// </summary>
        public ushort FunctionCode { get; set; }
        /// <summary>
        /// 会话 ID（请求/响应对应）
        /// </summary>
        public uint Id { get; set; }
        /// <summary>
        /// 数据域长度（数据）
        /// </summary>
        public int Length
        {
            get { return Data != null ? Data.Length : 0; }
        }
        /// <summary>
        /// 数据域
        /// </summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();
        /// <summary>
        /// CRC32 校验码
        /// </summary>
        public uint Verify { get; set; }

        /// <summary>获取数据域的 UTF-8 字符串</summary>
        public string GetStringData() => Encoding.UTF8.GetString(Data);

        public override string ToString()
            => $"[0x{Magic:X4}] Fn=0x{FunctionCode:X4} Id={Id} Len={Length} CRC=0x{Verify:X8}";

        /// <summary>
        /// 序列化为字节数组
        /// </summary>
        public static byte[] Serialize(DataPack frame)
        {
            // 总长度 = 帧头(2) + 功能码(2) + 会话ID(4) + 数据长度(4) + 数据域(N) + CRC32(4)
            int totalLen = 2 + 2 + 4 + 4 + frame.Length + 4;

            byte[] buffer = new byte[totalLen];
            int offset = 0;

            // 帧头
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset), frame.Magic);
            offset += 2;
            // 功能码
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset), frame.FunctionCode);
            offset += 2;
            // 会话 ID
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset), frame.Id);
            offset += 4;
            // 数据长度
            BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), frame.Length);
            offset += 4;
            // 数据域
            if (frame.Data.Length > 0)
            {
                Buffer.BlockCopy(frame.Data, 0, buffer, offset, frame.Data.Length);
                offset += frame.Data.Length;
            }

            // CRC32 校验码（仅校验 Data 域）
            uint crc = CrcCalculator.CalculateCrc32(frame.Data);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset), crc);

            return buffer;
        }

        /// <summary>
        /// 从 RingBuffer 中反序列化一帧（需先调用 EnsureAsync 确保数据足够）
        /// </summary>
        public static DataPack Deserialize(RingBuffer ring)
        {
            // 1. 读取帧头 (12 字节)
            var headerBytes = ring.Peek(ProtocolConstants.HeaderSize);

            ushort magic = BinaryPrimitives.ReadUInt16LittleEndian(headerBytes);
            if (magic != ProtocolConstants.FrameHeader)
                throw new InvalidDataException($"帧头魔数错误: 0x{magic:X4}，期望 0x{ProtocolConstants.FrameHeader:X4}");

            ushort fn = BinaryPrimitives.ReadUInt16LittleEndian(headerBytes.AsSpan(2));
            uint id = BinaryPrimitives.ReadUInt32LittleEndian(headerBytes.AsSpan(4));
            int length = BinaryPrimitives.ReadInt32LittleEndian(headerBytes.AsSpan(8));

            // 消费帧头
            ring.Skip(ProtocolConstants.HeaderSize);

            // 2. 读取数据域
            byte[] data = length > 0 ? ring.Read(length) : Array.Empty<byte>();

            // 3. 读取并校验 CRC32
            var verifyBytes = ring.Read(ProtocolConstants.VerifySize);
            uint receivedVerify = BinaryPrimitives.ReadUInt32LittleEndian(verifyBytes);

            uint computed = CrcCalculator.CalculateCrc32(data);
            if (receivedVerify != computed)
                throw new InvalidDataException($"CRC32 校验失败: 收到 0x{receivedVerify:X8}，计算 0x{computed:X8}");

            return new DataPack
            {
                Magic = magic,
                FunctionCode = fn,
                Id = id,
                Data = data,
                Verify = receivedVerify
            };
        }

        /// <summary>
        /// 发送帧到流（线程安全，自动序列化+CRC）
        /// </summary>
        public static async Task SendAsync(Stream stream, DataPack frame, SemaphoreSlim writeLock, CancellationToken ct = default)
        {
            var packet = Serialize(frame);
            await writeLock.WaitAsync(ct);
            try
            {
                await stream.WriteAsync(packet, ct);
                await stream.FlushAsync(ct);
            }
            finally
            {
                writeLock.Release();
            }
        }

        /// <summary>
        /// 从流中接收一帧（基于 RingBuffer，处理粘包/半包）
        /// </summary>
        public static async Task<DataPack> ReceiveAsync(Stream stream, RingBuffer ring, CancellationToken ct = default)
        {
            // 1. 确保读够帧头 (12 字节)
            await ring.EnsureAsync(stream, ProtocolConstants.HeaderSize, ct);

            // 2. 偷看帧头获取数据域长度
            var headerBytes = ring.Peek(ProtocolConstants.HeaderSize);
            int length = BinaryPrimitives.ReadInt32LittleEndian(headerBytes.AsSpan(8));

            // 3. 确保读够完整帧（帧头 + 数据域 + CRC32）
            // Deserialize 会 Skip(12) + Read(length) + Read(4)，需要总 unread >= 12+length+4
            int totalNeeded = ProtocolConstants.HeaderSize + length + ProtocolConstants.VerifySize;
            await ring.EnsureAsync(stream, totalNeeded, ct);

            // 4. 反序列化
            return Deserialize(ring);
        }
    }
}
