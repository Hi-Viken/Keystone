using System;
using System.Collections.Generic;
using System.Text;

namespace KepCommon
{
    /// <summary>协议常量</summary>
    public static class ProtocolConstants
    {
        /// <summary>帧头魔数</summary>
        public const ushort FrameHeader = 0xA55A;

        // ---- 功能码定义 ----
        public const ushort VersionList = 0x0001; // 服务器下发版本列表
        public const ushort VersionSelect = 0x0002; // 客户端选择版本
        public const ushort AuthRequest = 0x0003; // 身份认证请求
        public const ushort AuthResponse = 0x0004; // 身份认证响应
        public const ushort HeartbeatPing = 0x0005; // 心跳请求
        public const ushort HeartbeatPong = 0x0006; // 心跳响应
        public const ushort TimeSyncRequest = 0x0007; // 对时请求
        public const ushort TimeSyncResponse = 0x0008; // 对时响应
        public const ushort TempUpload = 0x0009; // 温度上报
        public const ushort TempAck = 0x000A; // 温度上报确认
        public const ushort ParamRequest = 0x000B; // 参数下发请求（服务端→客户端，需等待回复）
        public const ushort ParamResponse = 0x000C; // 参数下发回复（客户端→服务端，确认收到）
        public const ushort ClientRequest = 0x000D; // 客户端上报请求（客户端→服务端，需等待回复）
        public const ushort ClientResponse = 0x000E; // 客户端上报回复（服务端→客户端，确认收到）
        public const ushort FileDownload = 0x000F; // 文件下发（服务端→客户端）
        public const ushort FileDownloadAck = 0x0010; // 文件下发确认（客户端→服务端）
        public const ushort FileUpload = 0x0011; // 文件上传（客户端→服务端）
        public const ushort FileUploadAck = 0x0012; // 文件上传确认（服务端→客户端）

        /// <summary>帧头固定长度 (2+2+4+4 = 12 字节, 不含数据域和CRC)</summary>
        public const int HeaderSize = 12;
        /// <summary>CRC32 校验码长度</summary>
        public const int VerifySize = 4;

        /// <summary>文件分片最大数据载荷（字节），预留 16 字节分片头（文件名长度4+文件总大小4+偏移4+片长4）</summary>
        public const int FileChunkMaxPayload = 4096;
        /// <summary>分片头固定长度（文件名长度4 + 文件总大小4 + 偏移4 + 片长4 = 16）</summary>
        public const int FileChunkHeaderSize = 16;
    }

}
