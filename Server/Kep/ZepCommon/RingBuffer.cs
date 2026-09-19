namespace ZepCommon
{
    /// <summary>
    /// 固定容量环形缓冲区，用于从 Stream 流式读取并拼装完整帧。
    /// 支持跨边界读取（数据在缓冲区首尾回绕的情况）。
    /// </summary>
    public class RingBuffer
    {
        private readonly byte[] _buffer;
        private int _readPos;
        private int _writePos;

        public RingBuffer(int capacity = 8192)
        {
            _buffer = new byte[capacity];
        }

        public int Capacity => _buffer.Length;

        /// <summary>可读取的字节数</summary>
        public int Unread => (_writePos - _readPos + Capacity) % Capacity;

        /// <summary>可写入的字节数（保留 1 字节哨兵位）</summary>
        public int Free => Capacity - 1 - Unread;

        /// <summary>从缓冲区读取一个字节</summary>
        public byte ReadByte()
        {
            if (Unread == 0) throw new InvalidOperationException("缓冲区为空");
            byte b = _buffer[_readPos];
            _readPos = (_readPos + 1) % Capacity;
            return b;
        }

        /// <summary>从缓冲区读取指定长度的字节</summary>
        public byte[] Read(int count)
        {
            if (count > Unread) throw new InvalidOperationException($"需要 {count} 字节，仅有 {Unread}");
            var result = new byte[count];
            int r = _readPos;
            for (int i = 0; i < count; i++)
            {
                result[i] = _buffer[r];
                r = (r + 1) % Capacity;
            }
            _readPos = r;
            return result;
        }

        /// <summary>偷看缓冲区前 N 字节（不消费，不移动读指针）</summary>
        public byte[] Peek(int count)
        {
            if (count > Unread) throw new InvalidOperationException($"需要 {count} 字节，仅有 {Unread}");
            var result = new byte[count];
            int r = _readPos;
            for (int i = 0; i < count; i++)
            {
                result[i] = _buffer[r];
                r = (r + 1) % Capacity;
            }
            return result;
        }

        /// <summary>跳过指定数量的已读字节（不实际读取）</summary>
        public void Skip(int count)
        {
            if (count > Unread) throw new InvalidOperationException("跳过字节数超过可读数据");
            _readPos = (_readPos + count) % Capacity;
        }

        /// <summary>将已读数据写回缓冲区起始位置（当数据跨边界时用于展平）</summary>
        public void Compact()
        {
            if (_readPos == 0) return;
            int unread = Unread;
            if (unread == 0)
            {
                _readPos = 0;
                _writePos = 0;
                return;
            }

            var temp = new byte[unread];
            int r = _readPos;
            for (int i = 0; i < unread; i++)
            {
                temp[i] = _buffer[r];
                r = (r + 1) % Capacity;
            }
            temp.CopyTo(_buffer, 0);
            _readPos = 0;
            _writePos = unread;
        }

        /// <summary>确保缓冲区有足够可读数据，不足则从流中读取</summary>
        public async Task EnsureAsync(Stream stream, int needed, CancellationToken ct = default)
        {
            while (Unread < needed)
            {
                if (Free < 1)
                {
                    Compact();
                    if (Free < 1)
                        throw new InvalidOperationException($"环形缓冲区容量不足，需要 {needed} 字节");
                }

                int writeStart = _writePos;
                int contiguousFree;
                if (_writePos >= _readPos)
                    contiguousFree = Capacity - _writePos - (_readPos == 0 ? 1 : 0);
                else
                    contiguousFree = _readPos - _writePos - 1;

                if (contiguousFree <= 0)
                {
                    Compact();
                    continue;
                }

                int toRead = Math.Min(contiguousFree, needed - Unread);
                int read = await stream.ReadAsync(_buffer.AsMemory(writeStart, toRead), ct);
                if (read == 0) throw new EndOfStreamException("连接已断开");
                _writePos = (writeStart + read) % Capacity;
            }
        }

        /// <summary>重置缓冲区</summary>
        public void Reset()
        {
            _readPos = 0;
            _writePos = 0;
        }
    }
}
