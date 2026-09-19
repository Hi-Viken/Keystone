using SkyFrpPanel.Model;
using SkyFrpPanel.Model.Kep.Dto;
using SkyFrpPanel.Model.Kep.Model;

namespace SkyFrpPanel.ServiceCore.Kep
{
    public interface IFileManagementService : IBaseService<FileManagement>
    {
        /// <summary>
        /// 查询文件管理列表（分页）
        /// </summary>
        PagedInfo<FileManagement> SelectFileManagementList(FileManagementQueryDto dto);

        /// <summary>
        /// 根据ID获取详细信息
        /// </summary>
        FileManagement SelectFileManagementById(long id);

        /// <summary>
        /// 新增文件/目录
        /// </summary>
        long InsertFileManagement(FileManagement fileManagement);

        /// <summary>
        /// 修改文件/目录
        /// </summary>
        int UpdateFileManagement(FileManagement fileManagement);

        /// <summary>
        /// 批量删除文件/目录
        /// </summary>
        int DeleteFileManagementByIds(long[] ids);

        /// <summary>
        /// 获取所有文件/目录（导出用）
        /// </summary>
        List<FileManagement> SelectFileManagementAll();

        /// <summary>
        /// 查询文件管理树形列表
        /// </summary>
        List<FileManagement> SelectFileManagementTreeList(FileManagementQueryDto dto);

        /// <summary>
        /// 查询文件管理列表（排除指定节点及其子节点）
        /// </summary>
        List<FileManagement> SelectFileManagementListExcludeChild(long id);

        /// <summary>
        /// 按父级ID分页查询当前目录内容
        /// </summary>
        PagedInfo<FileManagement> SelectFileManagementByParentId(FileManagementQueryDto dto);

        /// <summary>
        /// 获取面包屑路径
        /// </summary>
        List<FileManagement> GetBreadcrumbPath(long id);

        /// <summary>
        /// 检查同级目录/文件名是否重复
        /// </summary>
        bool CheckDuplicateName(long parentId, string fileName, long excludeId = 0);
    }
}
