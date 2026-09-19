using SkyFrpPanel.Model.System;

namespace SkyFrpPanel.Model.Kep.Model
{
    /// <summary>
    /// 材料类型表
    /// </summary>
    [SugarTable("MaterialType", "材料类型表")]
    [Tenant("0")]
    public class MaterialType : SysBase
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true, ColumnName = "Id")]
        public long Id { get; set; }

        /// <summary>
        /// 父级ID（0表示顶级）
        /// </summary>
        [SugarColumn(ColumnDescription = "父级ID", DefaultValue = "0", ColumnName = "ParentId")]
        public long ParentId { get; set; }

        /// <summary>
        /// 材料类型编码（自动生成，6位唯一）
        /// </summary>
        [SugarColumn(Length = 6, ColumnDescription = "材料类型编码", ExtendedAttribute = ProteryConstant.NOTNULL, ColumnName = "Code")]
        public string Code { get; set; }

        /// <summary>
        /// 材料类型名称
        /// </summary>
        [SugarColumn(Length = 100, ColumnDescription = "材料类型名称", ExtendedAttribute = ProteryConstant.NOTNULL, ColumnName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [SugarColumn(Length = 500, ColumnDescription = "描述", IsNullable = true, ColumnName = "Description")]
        public string Description { get; set; }

        /// <summary>
        /// 子节点列表（非数据库字段）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<MaterialType> children { get; set; }
    }
}
