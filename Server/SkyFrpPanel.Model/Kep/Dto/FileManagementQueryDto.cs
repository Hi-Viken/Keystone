namespace SkyFrpPanel.Model.Kep.Dto
{
    /// <summary>
    /// 文件管理查询DTO
    /// </summary>
    public class FileManagementQueryDto : PagerInfo
    {
        /// <summary>
        /// 父级ID（用于按目录查询）
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 文件类型（0-目录，1-文件）
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// 文件/目录名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string FileExtension { get; set; }
    }
}
