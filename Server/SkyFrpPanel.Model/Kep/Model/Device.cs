using SkyFrpPanel.Model.System;

namespace SkyFrpPanel.Model.Kep.Model
{
    /// <summary>
    /// 设备表
    /// </summary>
    [SugarTable("Device", "设备表")]
    [Tenant("0")]
    public class Device : SysBase
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true, ColumnName = "Id")]
        public long Id { get; set; }

        /// <summary>
        /// 设备唯一码
        /// </summary>
        [SugarColumn(Length = 30, ColumnDescription = "设备唯一码", ExtendedAttribute = ProteryConstant.NOTNULL, ColumnName = "SN")]
        public string SN { get; set; }

        /// <summary>
        /// 设备密码
        /// </summary>
        [SugarColumn(Length = 180, ColumnDescription = "设备密码", ExtendedAttribute = ProteryConstant.NOTNULL, ColumnName = "Token")]
        public string Token { get; set; }

        /// <summary>
        /// 设备私钥
        /// </summary>
        [SugarColumn(Length = 180, ColumnDescription = "设备私钥", ExtendedAttribute = ProteryConstant.NOTNULL, ColumnName = "Secret")]
        public string Secret { get; set; }

        /// <summary>
        /// 最后上线时间
        /// </summary>
        [SugarColumn(ColumnDescription = "最后上线时间", IsNullable = true, ColumnName = "Last_online_at")]
        public DateTime? LastOnlineAt { get; set; }

        /// <summary>
        /// 最后离线时间
        /// </summary>
        [SugarColumn(ColumnDescription = "最后离线时间", IsNullable = true, ColumnName = "Last_offline_at")]
        public DateTime? LastOfflineAt { get; set; }

        /// <summary>
        /// 固件版本
        /// </summary>
        [SugarColumn(Length = 30, ColumnDescription = "固件版本", ExtendedAttribute = ProteryConstant.NOTNULL, ColumnName = "Firmware_version")]
        public string FirmwareVersion { get; set; }

        /// <summary>
        /// 硬件版本
        /// </summary>
        [SugarColumn(Length = 30, ColumnDescription = "硬件版本", ExtendedAttribute = ProteryConstant.NOTNULL, ColumnName = "Hardware_version")]
        public string HardwareVersion { get; set; }
    }
}
