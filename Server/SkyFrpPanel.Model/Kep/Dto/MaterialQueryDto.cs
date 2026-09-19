namespace SkyFrpPanel.Model.Kep.Dto
{
    /// <summary>
    /// 材料管理查询DTO
    /// </summary>
    public class MaterialQueryDto : PagerInfo
    {
        /// <summary>
        /// 材料编号
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 材料名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 材料类型ID
        /// </summary>
        public long? MaterialTypeId { get; set; }
    }
}
