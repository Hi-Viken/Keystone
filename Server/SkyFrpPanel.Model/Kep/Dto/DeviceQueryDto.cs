namespace SkyFrpPanel.Model.Kep.Dto
{
    /// <summary>
    /// 设备查询DTO
    /// </summary>
    public class DeviceQueryDto : PagerInfo
    {
        /// <summary>
        /// 设备唯一码
        /// </summary>
        public string SN { get; set; }

        /// <summary>
        /// 固件版本
        /// </summary>
        public string FirmwareVersion { get; set; }

        /// <summary>
        /// 硬件版本
        /// </summary>
        public string HardwareVersion { get; set; }
    }
}
