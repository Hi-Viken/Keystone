using System;
using System.Collections.Generic;
using System.Text;

namespace ZepCommon
{
    /// <summary>
    /// CRC32 计算器
    /// </summary>
    public static class CrcCalculator
    {
        private static readonly uint[] Table;

        static CrcCalculator()
        {
            Table = new uint[256];
            const uint poly = 0xEDB88320;
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int j = 0; j < 8; j++)
                {
                    crc = (crc >> 1) ^ ((crc & 1) != 0 ? poly : 0);
                }
                Table[i] = crc;
            }
        }

        public static uint CalculateCrc32(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            foreach (var b in data)
            {
                crc = (crc >> 8) ^ Table[(crc & 0xFF) ^ b];
            }
            return crc ^ 0xFFFFFFFF;
        }
    }
}
