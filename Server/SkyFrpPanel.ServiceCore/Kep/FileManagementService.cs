using SkyFrpPanel.Infrastructure;
using SkyFrpPanel.Infrastructure.Attribute;
using SkyFrpPanel.Model;
using SkyFrpPanel.Model.Kep.Dto;
using SkyFrpPanel.Model.Kep.Model;
using SkyFrpPanel.Repository;
using SqlSugar;

namespace SkyFrpPanel.ServiceCore.Kep
{
    /// <summary>
    /// 文件管理
    /// </summary>
    [AppService(ServiceType = typeof(IFileManagementService), ServiceLifetime = LifeTime.Transient)]
    public class FileManagementService : BaseService<FileManagement>, IFileManagementService
    {
        /// <summary>
        /// 查询文件管理列表（分页）
        /// </summary>
        public PagedInfo<FileManagement> SelectFileManagementList(FileManagementQueryDto dto)
        {
            var predicate = QueryExp(dto);
            var result = Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(it => it.Id)
                .ToPage(dto);
            return result;
        }

        /// <summary>
        /// 根据ID获取详细信息
        /// </summary>
        public FileManagement SelectFileManagementById(long id)
        {
            return GetId(id);
        }

        /// <summary>
        /// 新增文件/目录
        /// </summary>
        public long InsertFileManagement(FileManagement fileManagement)
        {
            return InsertReturnBigIdentity(fileManagement);
        }

        /// <summary>
        /// 修改文件/目录
        /// </summary>
        public int UpdateFileManagement(FileManagement fileManagement)
        {
            return Update(fileManagement);
        }

        /// <summary>
        /// 批量删除文件/目录
        /// </summary>
        public int DeleteFileManagementByIds(long[] ids)
        {
            return Delete(ids);
        }

        /// <summary>
        /// 获取所有文件/目录（导出用）
        /// </summary>
        public List<FileManagement> SelectFileManagementAll()
        {
            return Queryable().OrderByDescending(it => it.Id).ToList();
        }

        /// <summary>
        /// 查询文件管理树形列表
        /// </summary>
        public List<FileManagement> SelectFileManagementTreeList(FileManagementQueryDto dto)
        {
            var predicate = QueryExp(dto);
            var list = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(it => it.FileType)
                .OrderBy(it => it.Id)
                .ToList();

            return BuildTree(list);
        }

        /// <summary>
        /// 查询文件管理列表（排除指定节点及其子节点）
        /// </summary>
        public List<FileManagement> SelectFileManagementListExcludeChild(long id)
        {
            var allList = Queryable().OrderBy(it => it.Id).ToList();
            var excludeIds = GetChildIds(allList, id);
            excludeIds.Add(id);
            return allList.Where(it => !excludeIds.Contains(it.Id)).ToList();
        }

        /// <summary>
        /// 按父级ID分页查询当前目录内容
        /// </summary>
        public PagedInfo<FileManagement> SelectFileManagementByParentId(FileManagementQueryDto dto)
        {
            var predicate = Expressionable.Create<FileManagement>();
            predicate = predicate.And(it => it.ParentId == dto.ParentId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.FileType), it => it.FileType == dto.FileType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.FileName), it => it.FileName.Contains(dto.FileName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.FileExtension), it => it.FileExtension.Contains(dto.FileExtension));

            var result = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(it => it.FileType)
                .OrderByDescending(it => it.Id)
                .ToPage(dto);
            return result;
        }

        /// <summary>
        /// 获取面包屑路径
        /// </summary>
        public List<FileManagement> GetBreadcrumbPath(long id)
        {
            var path = new List<FileManagement>();
            if (id == 0) return path;

            var current = GetId(id);
            while (current != null)
            {
                path.Insert(0, current);
                if (current.ParentId == 0) break;
                current = GetId(current.ParentId);
            }
            return path;
        }

        /// <summary>
        /// 检查同级目录/文件名是否重复
        /// </summary>
        public bool CheckDuplicateName(long parentId, string fileName, long excludeId = 0)
        {
            var predicate = Expressionable.Create<FileManagement>();
            predicate = predicate.And(it => it.ParentId == parentId);
            predicate = predicate.And(it => it.FileName == fileName);
            predicate = predicate.AndIF(excludeId > 0, it => it.Id != excludeId);
            return Queryable().Where(predicate.ToExpression()).Any();
        }

        /// <summary>
        /// 构建树形结构
        /// </summary>
        private List<FileManagement> BuildTree(List<FileManagement> list)
        {
            List<FileManagement> returnList = new List<FileManagement>();
            List<long> tempList = list.Select(f => f.Id).ToList();

            foreach (var item in list)
            {
                if (!tempList.Contains(item.ParentId))
                {
                    RecursionFn(list, item);
                    returnList.Add(item);
                }
            }

            if (!returnList.Any())
            {
                returnList = list;
            }
            return returnList;
        }

        /// <summary>
        /// 递归构建子节点
        /// </summary>
        private void RecursionFn(List<FileManagement> list, FileManagement t)
        {
            List<FileManagement> childList = GetChildList(list, t);
            t.children = childList;
            foreach (var item in childList)
            {
                if (GetChildList(list, item).Any())
                {
                    RecursionFn(list, item);
                }
            }
        }

        /// <summary>
        /// 获取子节点列表
        /// </summary>
        private List<FileManagement> GetChildList(List<FileManagement> list, FileManagement parent)
        {
            return list.Where(p => p.ParentId == parent.Id).ToList();
        }

        /// <summary>
        /// 获取所有子节点ID（包括自身）
        /// </summary>
        private List<long> GetChildIds(List<FileManagement> list, long parentId)
        {
            List<long> childIds = new List<long>();
            var children = list.Where(p => p.ParentId == parentId).ToList();
            foreach (var child in children)
            {
                childIds.Add(child.Id);
                childIds.AddRange(GetChildIds(list, child.Id));
            }
            return childIds;
        }

        /// <summary>
        /// 构建查询条件
        /// </summary>
        private static Expressionable<FileManagement> QueryExp(FileManagementQueryDto dto)
        {
            var predicate = Expressionable.Create<FileManagement>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.FileType), it => it.FileType == dto.FileType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.FileName), it => it.FileName.Contains(dto.FileName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.FileExtension), it => it.FileExtension.Contains(dto.FileExtension));
            return predicate;
        }
    }
}
