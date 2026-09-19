using SkyFrpPanel.Model.System;

namespace SkyFrpPanel.Model.Kep.Model
{
    /// <summary>
    /// 材料管理表
    /// </summary>
    [SugarTable("Material", "材料管理表")]
    [Tenant("0")]
    public class Material : SysBase
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true, ColumnName = "Id")]
        public long Id { get; set; }

        /// <summary>
        /// 材料编号（自动生成，唯一）
        /// </summary>
        [SugarColumn(Length = 20, ColumnDescription = "材料编号", ExtendedAttribute = ProteryConstant.NOTNULL, ColumnName = "Code")]
        public string Code { get; set; }

        /// <summary>
        /// 材料名称
        /// </summary>
        [SugarColumn(Length = 200, ColumnDescription = "材料名称", ExtendedAttribute = ProteryConstant.NOTNULL, ColumnName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// 材料类型ID（关联MaterialType表）
        /// </summary>
        [SugarColumn(ColumnDescription = "材料类型ID", ColumnName = "MaterialTypeId")]
        public long MaterialTypeId { get; set; }

        /// <summary>
        /// 封面图片文件ID
        /// </summary>
        [SugarColumn(ColumnDescription = "封面图片文件ID", IsNullable = true, ColumnName = "CoverImageId")]
        public long? CoverImageId { get; set; }

        /// <summary>
        /// 封面图片路径（非数据库字段，用于前端显示）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string CoverImage { get; set; }

        /// <summary>
        /// 富文本内容（图文/视频描述）
        /// </summary>
        [SugarColumn(ColumnDescription = "富文本内容", IsNullable = true, ColumnName = "Content", Length = 5000)]
        public string Content { get; set; }

        /// <summary>
        /// 材料类型名称（非数据库字段，用于列表展示）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string MaterialTypeName { get; set; }

        /// <summary>
        /// 关联文件ID列表（非数据库字段，用于编辑时传入）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<long> FileIds { get; set; }

        /// <summary>
        /// 关联文件列表（非数据库字段，用于查询时返回）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<FileManagement> FileList { get; set; }
    }
}
