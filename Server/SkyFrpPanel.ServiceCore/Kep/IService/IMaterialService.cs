using SkyFrpPanel.Model;
using SkyFrpPanel.Model.Kep.Dto;
using SkyFrpPanel.Model.Kep.Model;

namespace SkyFrpPanel.ServiceCore.Kep
{
    public interface IMaterialService : IBaseService<Material>
    {
        /// <summary>
        /// 查询材料列表（分页）
        /// </summary>
        PagedInfo<Material> SelectMaterialList(MaterialQueryDto dto);

        /// <summary>
        /// 根据ID获取详细信息（包含关联文件）
        /// </summary>
        Material SelectMaterialById(long id);

        /// <summary>
        /// 新增材料（自动生成编号，关联文件）
        /// </summary>
        long InsertMaterial(Material material);

        /// <summary>
        /// 修改材料（更新关联文件）
        /// </summary>
        int UpdateMaterial(Material material);

        /// <summary>
        /// 批量删除材料（级联删除关联文件记录）
        /// </summary>
        int DeleteMaterialByIds(long[] ids);

        /// <summary>
        /// 校验编号是否唯一
        /// </summary>
        string CheckCodeUnique(Material material);

        /// <summary>
        /// 获取所有材料（导出用）
        /// </summary>
        List<Material> SelectMaterialAll();
    }
}
