namespace SkyFrpPanel.Model.Kep.Dto
{
    /// <summary>
    /// 节点查询DTO
    /// </summary>
    public class FrpNodeQueryDto : PagerInfo
    {
        /// <summary>
        /// 设备SN
        /// </summary>
        public string SN { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string NodeName { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? IsEnable { get; set; }
    }
}
