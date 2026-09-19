using Microsoft.AspNetCore.Mvc;
using SkyFrpPanel.Model.Kep.Dto;
using SkyFrpPanel.Model.Kep.Model;
using SkyFrpPanel.ServiceCore.Kep;

namespace SkyFrpPanel.Server.Controllers.Kep
{
    /// <summary>
    /// 材料管理
    /// </summary>
    [Route("Material/[action]")]
    public class MaterialController : BaseController
    {
        private readonly IMaterialService MaterialService;

        public MaterialController(IMaterialService materialService)
        {
            MaterialService = materialService;
        }

        /// <summary>
        /// 材料列表查询（分页）
        /// </summary>
        [HttpGet]
        [ActionPermissionFilter(Permission = "kep:material:list")]
        public IActionResult List([FromQuery] MaterialQueryDto dto)
        {
            var list = MaterialService.SelectMaterialList(dto);
            return SUCCESS(list);
        }

        /// <summary>
        /// 根据ID获取详细信息
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "kep:material:query")]
        public IActionResult Query(long id)
        {
            return SUCCESS(MaterialService.SelectMaterialById(id));
        }

        /// <summary>
        /// 添加材料
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "kep:material:add")]
        [Log(Title = "材料添加", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] Material material)
        {
            material.ToCreate(HttpContext);
            return ToResponse(MaterialService.InsertMaterial(material));
        }

        /// <summary>
        /// 修改材料
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "kep:material:edit")]
        [Log(Title = "材料编辑", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] Material material)
        {
            material.ToUpdate(HttpContext);
            return ToResponse(MaterialService.UpdateMaterial(material));
        }

        /// <summary>
        /// 删除材料
        /// </summary>
        [HttpDelete("{id}")]
        [ActionPermissionFilter(Permission = "kep:material:remove")]
        [Log(Title = "材料删除", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string id)
        {
            long[] ids = Tools.SpitLongArrary(id);
            return ToResponse(MaterialService.DeleteMaterialByIds(ids));
        }

        /// <summary>
        /// 材料导出
        /// </summary>
        [Log(BusinessType = BusinessType.EXPORT, IsSaveResponseData = false, Title = "材料导出")]
        [HttpGet()]
        [ActionPermissionFilter(Permission = "kep:material:export")]
        public IActionResult Export()
        {
            var list = MaterialService.SelectMaterialAll();
            var result = ExportExcelMini(list, "material", "材料列表");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}
