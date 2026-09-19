namespace SkyFrpPanel.Model.Kep.Dto
{
    /// <summary>
    /// 材料类型查询DTO
    /// </summary>
    public class MaterialTypeQueryDto : PagerInfo
    {
        /// <summary>
        /// 材料类型编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 材料类型名称
        /// </summary>
        public string Name { get; set; }
    }
}
