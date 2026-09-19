using SkyFrpPanel.Infrastructure;
using SkyFrpPanel.Infrastructure.Attribute;
using SkyFrpPanel.Model;
using SkyFrpPanel.Model.Kep.Dto;
using SkyFrpPanel.Model.Kep.Model;
using SkyFrpPanel.Model.System;
using SkyFrpPanel.Repository;
using SqlSugar;

namespace SkyFrpPanel.ServiceCore.Kep
{
    /// <summary>
    /// 材料管理
    /// </summary>
    [AppService(ServiceType = typeof(IMaterialService), ServiceLifetime = LifeTime.Transient)]
    public class MaterialService : BaseService<Material>, IMaterialService
    {
        private readonly IFileManagementService _fileManagementService;

        public MaterialService(IFileManagementService fileManagementService)
        {
            _fileManagementService = fileManagementService;
        }

        /// <summary>
        /// 查询材料列表（分页）
        /// </summary>
        public PagedInfo<Material> SelectMaterialList(MaterialQueryDto dto)
        {
            var predicate = QueryExp(dto);
            var result = Queryable()
                .LeftJoin<MaterialType>((m, mt) => m.MaterialTypeId == mt.Id)
                .Where(predicate.ToExpression())
                .Select((m, mt) => new Material
                {
                    Id = m.Id,
                    Code = m.Code,
                    Name = m.Name,
                    MaterialTypeId = m.MaterialTypeId,
                    CoverImageId = m.CoverImageId,
                    Content = m.Content,
                    MaterialTypeName = mt.Name,
                    Create_by = m.Create_by,
                    Create_time = m.Create_time,
                    Update_by = m.Update_by,
                    Update_time = m.Update_time
                })
                .OrderByDescending(m => m.Id)
                .ToPage(dto);

            // 填充封面图片下载URL
            foreach (var item in result.Result)
            {
                if (item.CoverImageId.HasValue)
                {
                    var coverFile = _fileManagementService.GetId(item.CoverImageId.Value);
                    if (coverFile != null)
                    {
                        item.CoverImage = $"/FileManagement/Download/{item.CoverImageId.Value}";
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 根据ID获取详细信息（包含关联文件）
        /// </summary>
        public Material SelectMaterialById(long id)
        {
            var material = GetId(id);
            if (material != null)
            {
                // 查询关联文件
                material.FileList = _fileManagementService.Queryable()
                    .Where(f => f.ParentId == id && f.FileType == "1")
                    .OrderByDescending(f => f.Id)
                    .ToList();

                // 填充封面图片下载URL
                if (material.CoverImageId.HasValue)
                {
                    material.CoverImage = $"/FileManagement/Download/{material.CoverImageId.Value}";
                }
            }
            return material;
        }

        /// <summary>
        /// 新增材料（自动生成编号，关联文件）
        /// </summary>
        public long InsertMaterial(Material material)
        {
            // 自动生成编号
            if (string.IsNullOrEmpty(material.Code))
            {
                material.Code = GenerateCode();
            }

            if (UserConstants.NOT_UNIQUE.Equals(CheckCodeUnique(material)))
                throw new CustomException($"添加材料'{material.Name}'失败，编号已存在");

            var id = InsertReturnBigIdentity(material);

            // 关联文件
            if (material.FileIds != null && material.FileIds.Any())
            {
                LinkFiles(id, material.FileIds);
            }

            // 关联封面图片
            if (material.CoverImageId.HasValue)
            {
                var coverFile = _fileManagementService.GetId(material.CoverImageId.Value);
                if (coverFile != null)
                {
                    coverFile.ParentId = id;
                    _fileManagementService.Update(coverFile);
                }
            }

            return id;
        }

        /// <summary>
        /// 修改材料（更新关联文件）
        /// </summary>
        public int UpdateMaterial(Material material)
        {
            if (UserConstants.NOT_UNIQUE.Equals(CheckCodeUnique(material)))
                throw new CustomException($"修改材料'{material.Name}'失败，编号已存在");

            var result = Update(material);

            // 更新关联文件（先清除旧关联，再建立新关联）
            UnlinkFiles(material.Id);
            if (material.FileIds != null && material.FileIds.Any())
            {
                LinkFiles(material.Id, material.FileIds);
            }

            return result;
        }

        /// <summary>
        /// 批量删除材料（级联删除关联文件记录）
        /// </summary>
        public int DeleteMaterialByIds(long[] ids)
        {
            // 先删除关联的文件记录
            foreach (var id in ids)
            {
                UnlinkFiles(id);
            }

            return Delete(ids);
        }

        /// <summary>
        /// 校验编号是否唯一
        /// </summary>
        public string CheckCodeUnique(Material material)
        {
            Material info = GetFirst(it => it.Code.Equals(material.Code));
            if (info != null && info.Id != material.Id)
                return UserConstants.NOT_UNIQUE;
            return UserConstants.UNIQUE;
        }

        /// <summary>
        /// 获取所有材料（导出用）
        /// </summary>
        public List<Material> SelectMaterialAll()
        {
            return Queryable()
                .LeftJoin<MaterialType>((m, mt) => m.MaterialTypeId == mt.Id)
                .Select((m, mt) => new Material
                {
                    Id = m.Id,
                    Code = m.Code,
                    Name = m.Name,
                    MaterialTypeId = m.MaterialTypeId,
                    Content = m.Content,
                    MaterialTypeName = mt.Name,
                    Create_by = m.Create_by,
                    Create_time = m.Create_time
                })
                .OrderByDescending(m => m.Id)
                .ToList();
        }

        /// <summary>
        /// 关联文件（将文件的ParentId设置为材料ID）
        /// </summary>
        private void LinkFiles(long materialId, List<long> fileIds)
        {
            foreach (var fileId in fileIds)
            {
                var file = _fileManagementService.GetId(fileId);
                if (file != null && file.FileType == "1")
                {
                    file.ParentId = materialId;
                    _fileManagementService.Update(file);
                }
            }
        }

        /// <summary>
        /// 取消关联文件（将文件的ParentId设置为0）
        /// </summary>
        private void UnlinkFiles(long materialId)
        {
            var files = _fileManagementService.Queryable()
                .Where(f => f.ParentId == materialId && f.FileType == "1")
                .ToList();

            foreach (var file in files)
            {
                file.ParentId = 0;
                _fileManagementService.Update(file);
            }
        }

        /// <summary>
        /// 生成唯一编号（格式：MAT + 5位序号，如 MAT00001）
        /// </summary>
        private string GenerateCode()
        {
            var maxMaterial = Queryable()
                .Where(it => it.Code.StartsWith("MAT"))
                .OrderByDescending(it => it.Code)
                .First();

            int nextSeq = 1;
            if (maxMaterial != null && !string.IsNullOrEmpty(maxMaterial.Code))
            {
                string seqStr = maxMaterial.Code.Substring(3);
                if (int.TryParse(seqStr, out int currentSeq))
                {
                    nextSeq = currentSeq + 1;
                }
            }

            return $"MAT{nextSeq:D5}";
        }

        /// <summary>
        /// 构建查询条件
        /// </summary>
        private static Expressionable<Material> QueryExp(MaterialQueryDto dto)
        {
            var predicate = Expressionable.Create<Material>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Code), it => it.Code.Contains(dto.Code));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Name), it => it.Name.Contains(dto.Name));
            predicate = predicate.AndIF(dto.MaterialTypeId.HasValue && dto.MaterialTypeId.Value > 0, it => it.MaterialTypeId == dto.MaterialTypeId.Value);
            return predicate;
        }
    }
}
