using SqlSugar;

namespace SkyFrpPanel.Model.Kep.Dto
{
    /// <summary>
    /// 材料类型响应DTO（包含创建人和修改人昵称）
    /// </summary>
    public class MaterialTypeResponseDto
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 父级ID
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 材料类型编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 材料类型名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string Create_by { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime Create_time { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string Update_by { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? Update_time { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建人昵称
        /// </summary>
        public string CreateByNickName { get; set; }

        /// <summary>
        /// 修改人昵称
        /// </summary>
        public string UpdateByNickName { get; set; }

        /// <summary>
        /// 子节点列表
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<MaterialTypeResponseDto> children { get; set; }
    }
}
