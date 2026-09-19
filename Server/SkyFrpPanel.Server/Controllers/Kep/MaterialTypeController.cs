using Microsoft.AspNetCore.Mvc;
using SkyFrpPanel.Model.Kep.Dto;
using SkyFrpPanel.Model.Kep.Model;
using SkyFrpPanel.ServiceCore.Kep;

namespace SkyFrpPanel.Server.Controllers.Kep
{
    /// <summary>
    /// 材料类型管理
    /// </summary>
    [Route("MaterialType/[action]")]
    public class MaterialTypeController : BaseController
    {
        private readonly IMaterialTypeService MaterialTypeService;

        public MaterialTypeController(IMaterialTypeService materialTypeService)
        {
            MaterialTypeService = materialTypeService;
        }

        /// <summary>
        /// 材料类型列表查询（分页）
        /// </summary>
        [HttpGet]
        [ActionPermissionFilter(Permission = "kep:materialtype:list")]
        public IActionResult List([FromQuery] MaterialTypeQueryDto dto)
        {
            var list = MaterialTypeService.SelectMaterialTypeList(dto);
            return SUCCESS(list);
        }

        /// <summary>
        /// 材料类型树形列表查询
        /// </summary>
        [HttpGet]
        [ActionPermissionFilter(Permission = "kep:materialtype:list")]
        public IActionResult TreeList([FromQuery] MaterialTypeQueryDto dto)
        {
            var list = MaterialTypeService.SelectMaterialTypeTreeList(dto);
            return SUCCESS(list);
        }

        /// <summary>
        /// 材料类型列表（排除指定节点及其子节点）
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "kep:materialtype:list")]
        public IActionResult ExcludeChild(long id = 0)
        {
            var list = MaterialTypeService.SelectMaterialTypeListExcludeChild(id);
            return SUCCESS(list);
        }

        /// <summary>
        /// 根据ID获取详细信息
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "kep:materialtype:query")]
        public IActionResult Query(long id = 0)
        {
            return SUCCESS(MaterialTypeService.SelectMaterialTypeById(id));
        }

        /// <summary>
        /// 添加材料类型
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "kep:materialtype:add")]
        [Log(Title = "材料类型添加", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] MaterialType materialType)
        {
            materialType.ToCreate(HttpContext);
            return ToResponse(MaterialTypeService.InsertMaterialType(materialType));
        }

        /// <summary>
        /// 修改材料类型
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "kep:materialtype:edit")]
        [Log(Title = "材料类型编辑", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] MaterialType materialType)
        {
            materialType.ToUpdate(HttpContext);
            return ToResponse(MaterialTypeService.UpdateMaterialType(materialType));
        }

        /// <summary>
        /// 删除材料类型
        /// </summary>
        [HttpDelete("{id}")]
        [ActionPermissionFilter(Permission = "kep:materialtype:remove")]
        [Log(Title = "材料类型删除", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string id)
        {
            long[] ids = Tools.SpitLongArrary(id);
            return ToResponse(MaterialTypeService.DeleteMaterialTypeByIds(ids));
        }

        /// <summary>
        /// 材料类型导出
        /// </summary>
        [Log(BusinessType = BusinessType.EXPORT, IsSaveResponseData = false, Title = "材料类型导出")]
        [HttpGet()]
        [ActionPermissionFilter(Permission = "kep:materialtype:export")]
        public IActionResult Export()
        {
            var list = MaterialTypeService.SelectMaterialTypeAll();
            var result = ExportExcelMini(list, "materialtype", "材料类型列表");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}
