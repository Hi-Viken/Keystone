using KepCommon;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Text;

namespace ZepCommon
{
    /// <summary>
    /// 分片文件重组跟踪器，用于接收端按 Id 重组分片文件。
    /// </summary>
    public class PartialFileTracker
    {
        private readonly ConcurrentDictionary<uint, FileAssemblyContext> _assemblies = new();

        /// <summary>
        /// 处理一个分片帧，返回是否已完成重组（true 表示文件已完整接收）
        /// </summary>
        /// <param name="frame">分片帧</param>
        /// <param name="completedFilePath">完成时的文件路径</param>
        /// <param name="completedFileData">完成时的文件数据</param>
        /// <param name="receivedBytes">当前已接收字节数</param>
        /// <param name="totalSize">文件总字节数</param>
        public bool HandleChunk(DataPack frame, out string? completedFilePath, out byte[]? completedFileData,
            out long receivedBytes, out long totalSize)
        {
            completedFilePath = null;
            completedFileData = null;
            receivedBytes = 0;
            totalSize = 0;

            byte[] data = frame.Data;
            if (data.Length < ProtocolConstants.FileChunkHeaderSize)
                throw new InvalidDataException("分片数据长度不足");

            // 解析分片头
            int fileNameLen = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(0));
            if (data.Length < 4 + fileNameLen + 12)
                throw new InvalidDataException("分片头格式错误");

            string fileName = Encoding.UTF8.GetString(data, 4, fileNameLen);
            int offsetAfterName = 4 + fileNameLen;

            long fileSize = BinaryPrimitives.ReadInt64LittleEndian(data.AsSpan(offsetAfterName));
            long chunkOffset = BinaryPrimitives.ReadInt64LittleEndian(data.AsSpan(offsetAfterName + 8));
            int chunkLength = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offsetAfterName + 16));

            int dataStart = offsetAfterName + 20;
            if (data.Length < dataStart + chunkLength)
                throw new InvalidDataException("分片数据长度不足");

            // 获取或创建重组上下文
            var ctx = _assemblies.GetOrAdd(frame.Id, _ => new FileAssemblyContext(fileName, fileSize));

            // 写入分片数据
            byte[] chunkData = new byte[chunkLength];
            Buffer.BlockCopy(data, dataStart, chunkData, 0, chunkLength);
            ctx.WriteChunk(chunkOffset, chunkData);

            // 获取进度
            receivedBytes = ctx.ReceivedBytes;
            totalSize = ctx.TotalSize;

            // 检查是否完成
            if (ctx.IsComplete)
            {
                _assemblies.TryRemove(frame.Id, out _);
                completedFilePath = fileName;
                completedFileData = ctx.GetCompleteData();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 构建分片帧的 Data 域
        /// </summary>
        public static byte[] BuildChunkData(string fileName, long totalSize, long chunkOffset, byte[] chunkData)
        {
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
            // 格式: [4字节文件名长度][文件名][8字节文件总大小][8字节偏移][4字节片长][分片数据]
            int totalLen = 4 + fileNameBytes.Length + 8 + 8 + 4 + chunkData.Length;
            byte[] result = new byte[totalLen];
            int offset = 0;

            BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(offset), fileNameBytes.Length);
            offset += 4;
            Buffer.BlockCopy(fileNameBytes, 0, result, offset, fileNameBytes.Length);
            offset += fileNameBytes.Length;
            BinaryPrimitives.WriteInt64LittleEndian(result.AsSpan(offset), totalSize);
            offset += 8;
            BinaryPrimitives.WriteInt64LittleEndian(result.AsSpan(offset), chunkOffset);
            offset += 8;
            BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(offset), chunkData.Length);
            offset += 4;
            Buffer.BlockCopy(chunkData, 0, result, offset, chunkData.Length);

            return result;
        }

        /// <summary>
        /// 清理超时的重组上下文
        /// </summary>
        public void CleanupStale(TimeSpan timeout)
        {
            var now = DateTime.UtcNow;
            foreach (var kvp in _assemblies)
            {
                if (now - kvp.Value.LastActivity > timeout)
                {
                    _assemblies.TryRemove(kvp.Key, out _);
                }
            }
        }

        /// <summary>
        /// 文件重组上下文
        /// </summary>
        private class FileAssemblyContext
        {
            public string FileName { get; }
            public long TotalSize { get; }
            public DateTime LastActivity { get; private set; } = DateTime.UtcNow;

            private readonly byte[] _buffer;
            private long _receivedBytes;
            private readonly object _lock = new();

            public FileAssemblyContext(string fileName, long totalSize)
            {
                FileName = fileName;
                TotalSize = totalSize;
                _buffer = new byte[totalSize];
                _receivedBytes = 0;
            }

            public long ReceivedBytes
            {
                get
                {
                    lock (_lock)
                    {
                        return _receivedBytes;
                    }
                }
            }

            public bool IsComplete
            {
                get
                {
                    lock (_lock)
                    {
                        return _receivedBytes >= TotalSize;
                    }
                }
            }

            public void WriteChunk(long offset, byte[] data)
            {
                lock (_lock)
                {
                    if (offset + data.Length > TotalSize)
                        throw new InvalidDataException($"分片越界: offset={offset}, len={data.Length}, total={TotalSize}");

                    Buffer.BlockCopy(data, 0, _buffer, (int)offset, data.Length);
                    _receivedBytes += data.Length;
                    LastActivity = DateTime.UtcNow;
                }
            }

            public byte[] GetCompleteData()
            {
                lock (_lock)
                {
                    if (_receivedBytes != TotalSize)
                        throw new InvalidOperationException("文件尚未完整接收");
                    return _buffer;
                }
            }
        }
    }
}
