using SkyFrpPanel.Model.System;

namespace SkyFrpPanel.Model.Kep.Model
{
    /// <summary>
    /// 文件管理表
    /// </summary>
    [SugarTable("FileManagement", "文件管理表")]
    [Tenant("0")]
    public class FileManagement : SysBase
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
        /// 文件类型（0-目录，1-文件）
        /// </summary>
        [SugarColumn(Length = 1, ColumnDescription = "文件类型（0-目录，1-文件）", DefaultValue = "0", ColumnName = "FileType")]
        public string FileType { get; set; }

        /// <summary>
        /// 文件/目录名称
        /// </summary>
        [SugarColumn(Length = 200, ColumnDescription = "文件/目录名称", ExtendedAttribute = ProteryConstant.NOTNULL, ColumnName = "FileName")]
        public string FileName { get; set; }

        /// <summary>
        /// 文件路径（仅文件类型使用）
        /// </summary>
        [SugarColumn(Length = 500, ColumnDescription = "文件路径", IsNullable = true, ColumnName = "FilePath")]
        public string FilePath { get; set; }

        /// <summary>
        /// 文件大小（字节，仅文件类型使用）
        /// </summary>
        [SugarColumn(ColumnDescription = "文件大小（字节）", IsNullable = true, ColumnName = "FileSize")]
        public long? FileSize { get; set; }

        /// <summary>
        /// 文件扩展名（仅文件类型使用）
        /// </summary>
        [SugarColumn(Length = 20, ColumnDescription = "文件扩展名", IsNullable = true, ColumnName = "FileExtension")]
        public string FileExtension { get; set; }

        /// <summary>
        /// 文件哈希值（SHA256，仅文件类型使用）
        /// </summary>
        [SugarColumn(Length = 64, ColumnDescription = "文件哈希值", IsNullable = true, ColumnName = "FileHash")]
        public string FileHash { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [SugarColumn(Length = 500, ColumnDescription = "描述", IsNullable = true, ColumnName = "Description")]
        public string Description { get; set; }

        /// <summary>
        /// 子节点列表（非数据库字段）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<FileManagement> children { get; set; }
    }
}
