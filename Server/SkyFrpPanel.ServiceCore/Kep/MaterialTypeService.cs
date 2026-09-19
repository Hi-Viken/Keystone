using SkyFrpPanel.Infrastructure;
using SkyFrpPanel.Infrastructure.Attribute;
using SkyFrpPanel.Model;
using SkyFrpPanel.Model.Kep.Dto;
using SkyFrpPanel.Model.Kep.Model;
using SkyFrpPanel.Model.System;
using SkyFrpPanel.Model.System.Model;
using SkyFrpPanel.Repository;
using SqlSugar;

namespace SkyFrpPanel.ServiceCore.Kep
{
    /// <summary>
    /// 材料类型管理
    /// </summary>
    [AppService(ServiceType = typeof(IMaterialTypeService), ServiceLifetime = LifeTime.Transient)]
    public class MaterialTypeService : BaseService<MaterialType>, IMaterialTypeService
    {
        /// <summary>
        /// 查询材料类型列表（分页）
        /// </summary>
        public PagedInfo<MaterialTypeResponseDto> SelectMaterialTypeList(MaterialTypeQueryDto dto)
        {
            var predicate = QueryExp(dto);
            var query = Queryable()
                .LeftJoin<SysUser>((it, u) => it.Create_by == u.UserName)
                .LeftJoin<SysUser>((it, u, u2) => it.Update_by == u2.UserName)
                .Where(predicate.ToExpression())
                .OrderByDescending(it => it.Id)
                .Select((it, u, u2) => new MaterialTypeResponseDto
                {
                    Id = it.Id,
                    ParentId = it.ParentId,
                    Code = it.Code,
                    Name = it.Name,
                    Description = it.Description,
                    Create_by = it.Create_by,
                    Create_time = it.Create_time,
                    Update_by = it.Update_by,
                    Update_time = it.Update_time,
                    Remark = it.Remark,
                    CreateByNickName = u.NickName,
                    UpdateByNickName = u2.NickName
                });

            var total = 0;
            var list = query.ToPageList(dto.PageNum, dto.PageSize, ref total);

            var result = new PagedInfo<MaterialTypeResponseDto>
            {
                PageIndex = dto.PageNum,
                PageSize = dto.PageSize,
                TotalNum = total,
                Result = list
            };
            return result;
        }

        /// <summary>
        /// 根据ID获取详细信息
        /// </summary>
        public MaterialType SelectMaterialTypeById(long id)
        {
            return GetId(id);
        }

        /// <summary>
        /// 新增材料类型（自动生成编码）
        /// </summary>
        public long InsertMaterialType(MaterialType materialType)
        {
            // 自动生成6位编码
            if (string.IsNullOrEmpty(materialType.Code))
            {
                materialType.Code = GenerateCode();
            }

            if (UserConstants.NOT_UNIQUE.Equals(CheckCodeUnique(materialType)))
                throw new CustomException($"添加材料类型'{materialType.Name}'失败，编码已存在");

            return InsertReturnBigIdentity(materialType);
        }

        /// <summary>
        /// 修改材料类型
        /// </summary>
        public int UpdateMaterialType(MaterialType materialType)
        {
            if (UserConstants.NOT_UNIQUE.Equals(CheckCodeUnique(materialType)))
                throw new CustomException($"修改材料类型'{materialType.Name}'失败，编码已存在");

            return Update(materialType);
        }

        /// <summary>
        /// 批量删除材料类型
        /// </summary>
        public int DeleteMaterialTypeByIds(long[] ids)
        {
            return Delete(ids);
        }

        /// <summary>
        /// 校验编码是否唯一
        /// </summary>
        public string CheckCodeUnique(MaterialType materialType)
        {
            MaterialType info = GetFirst(it => it.Code.Equals(materialType.Code));
            if (info != null && info.Id != materialType.Id)
                return UserConstants.NOT_UNIQUE;
            return UserConstants.UNIQUE;
        }

        /// <summary>
        /// 获取所有材料类型（导出用）
        /// </summary>
        public List<MaterialType> SelectMaterialTypeAll()
        {
            return Queryable().OrderByDescending(it => it.Id).ToList();
        }

        /// <summary>
        /// 查询材料类型树形列表
        /// </summary>
        public List<MaterialTypeResponseDto> SelectMaterialTypeTreeList(MaterialTypeQueryDto dto)
        {
            var predicate = QueryExp(dto);
            var list = Queryable()
                .LeftJoin<SysUser>((it, u) => it.Create_by == u.UserName)
                .LeftJoin<SysUser>((it, u, u2) => it.Update_by == u2.UserName)
                .Where(predicate.ToExpression())
                .OrderBy(it => it.Id)
                .Select((it, u, u2) => new MaterialTypeResponseDto
                {
                    Id = it.Id,
                    ParentId = it.ParentId,
                    Code = it.Code,
                    Name = it.Name,
                    Description = it.Description,
                    Create_by = it.Create_by,
                    Create_time = it.Create_time,
                    Update_by = it.Update_by,
                    Update_time = it.Update_time,
                    Remark = it.Remark,
                    CreateByNickName = u.NickName,
                    UpdateByNickName = u2.NickName
                })
                .ToList();

            return BuildTree(list);
        }

        /// <summary>
        /// 查询材料类型列表（排除指定节点及其子节点）
        /// </summary>
        public List<MaterialType> SelectMaterialTypeListExcludeChild(long id)
        {
            var allList = Queryable().OrderBy(it => it.Id).ToList();
            var excludeIds = GetChildIds(allList, id);
            excludeIds.Add(id);
            return allList.Where(it => !excludeIds.Contains(it.Id)).ToList();
        }

        /// <summary>
        /// 构建树形结构
        /// </summary>
        private List<MaterialTypeResponseDto> BuildTree(List<MaterialTypeResponseDto> list)
        {
            List<MaterialTypeResponseDto> returnList = new List<MaterialTypeResponseDto>();
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
        private void RecursionFn(List<MaterialTypeResponseDto> list, MaterialTypeResponseDto t)
        {
            List<MaterialTypeResponseDto> childList = GetChildList(list, t);
            // 使用 DTO 的 children 属性（已用 new 关键字重新定义）
            ((MaterialTypeResponseDto)t).children = childList;
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
        private List<MaterialTypeResponseDto> GetChildList(List<MaterialTypeResponseDto> list, MaterialTypeResponseDto parent)
        {
            return list.Where(p => p.ParentId == parent.Id).ToList();
        }

        /// <summary>
        /// 获取所有子节点ID（包括自身）
        /// </summary>
        private List<long> GetChildIds(List<MaterialType> list, long parentId)
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
        /// 生成6位唯一编码（格式：MT + 4位序号，如 MT0001）
        /// </summary>
        private string GenerateCode()
        {
            // 获取当前最大编码的序号
            var maxMaterialType = Queryable()
                .Where(it => it.Code.StartsWith("MT"))
                .OrderByDescending(it => it.Code)
                .First();

            int nextSeq = 1;
            if (maxMaterialType != null && !string.IsNullOrEmpty(maxMaterialType.Code))
            {
                // 提取序号部分（去掉MT前缀）
                string seqStr = maxMaterialType.Code.Substring(2);
                if (int.TryParse(seqStr, out int currentSeq))
                {
                    nextSeq = currentSeq + 1;
                }
            }

            // 格式化为4位序号
            return $"MT{nextSeq:D4}";
        }

        /// <summary>
        /// 构建查询条件
        /// </summary>
        private static Expressionable<MaterialType> QueryExp(MaterialTypeQueryDto dto)
        {
            var predicate = Expressionable.Create<MaterialType>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Code), it => it.Code.Contains(dto.Code));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Name), it => it.Name.Contains(dto.Name));
            return predicate;
        }
    }
}
