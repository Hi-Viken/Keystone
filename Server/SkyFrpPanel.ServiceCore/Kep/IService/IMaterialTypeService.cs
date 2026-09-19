using SkyFrpPanel.Model;
using SkyFrpPanel.Model.Kep.Dto;
using SkyFrpPanel.Model.Kep.Model;

namespace SkyFrpPanel.ServiceCore.Kep
{
    public interface IMaterialTypeService : IBaseService<MaterialType>
    {
        /// <summary>
        /// 查询材料类型列表（分页）
        /// </summary>
        PagedInfo<MaterialTypeResponseDto> SelectMaterialTypeList(MaterialTypeQueryDto dto);

        /// <summary>
        /// 根据ID获取详细信息
        /// </summary>
        MaterialType SelectMaterialTypeById(long id);

        /// <summary>
        /// 新增材料类型
        /// </summary>
        long InsertMaterialType(MaterialType materialType);

        /// <summary>
        /// 修改材料类型
        /// </summary>
        int UpdateMaterialType(MaterialType materialType);

        /// <summary>
        /// 批量删除材料类型
        /// </summary>
        int DeleteMaterialTypeByIds(long[] ids);

        /// <summary>
        /// 校验编码是否唯一
        /// </summary>
        string CheckCodeUnique(MaterialType materialType);

        /// <summary>
        /// 获取所有材料类型（导出用）
        /// </summary>
        List<MaterialType> SelectMaterialTypeAll();

        /// <summary>
        /// 查询材料类型树形列表
        /// </summary>
        List<MaterialTypeResponseDto> SelectMaterialTypeTreeList(MaterialTypeQueryDto dto);

        /// <summary>
        /// 查询材料类型列表（排除指定节点及其子节点）
        /// </summary>
        List<MaterialType> SelectMaterialTypeListExcludeChild(long id);
    }
}
