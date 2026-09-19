using Microsoft.AspNetCore.Mvc;
using SkyFrpPanel.Model.Kep.Dto;
using SkyFrpPanel.Model.Kep.Model;
using SkyFrpPanel.ServiceCore.Kep;

namespace SkyFrpPanel.Server.Controllers.Kep
{
    /// <summary>
    /// 设备管理
    /// </summary>
    [Route("Device/[action]")]
    public class DeviceController : BaseController
    {
        private readonly IDeviceService DeviceService;

        public DeviceController(IDeviceService deviceService)
        {
            DeviceService = deviceService;
        }

        /// <summary>
        /// 设备列表查询
        /// </summary>
        [HttpGet]
        [ActionPermissionFilter(Permission = "kep:device:list")]
        public IActionResult List([FromQuery] DeviceQueryDto dto)
        {
            var list = DeviceService.SelectDeviceList(dto);
            return SUCCESS(list);
        }

        /// <summary>
        /// 根据设备ID获取详细信息
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "kep:device:query")]
        public IActionResult Query(long id = 0)
        {
            return SUCCESS(DeviceService.SelectDeviceById(id));
        }

        /// <summary>
        /// 根据设备ID获取Token明文
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "kep:device:query")]
        public IActionResult GetToken(long id)
        {
            return SUCCESS(new { token = DeviceService.SelectTokenById(id) });
        }

        /// <summary>
        /// 根据设备ID获取Secret明文
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "kep:device:query")]
        public IActionResult GetSecret(long id)
        {
            return SUCCESS(new { secret = DeviceService.SelectSecretById(id) });
        }

        /// <summary>
        /// 添加设备
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "kep:device:add")]
        [Log(Title = "设备添加", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] Device device)
        {
            device.ToCreate(HttpContext);
            return ToResponse(DeviceService.InsertDevice(device));
        }

        /// <summary>
        /// 修改设备
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "kep:device:edit")]
        [Log(Title = "设备编辑", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] Device device)
        {
            device.ToUpdate(HttpContext);
            return ToResponse(DeviceService.UpdateDevice(device));
        }

        /// <summary>
        /// 删除设备
        /// </summary>
        [HttpDelete("{id}")]
        [ActionPermissionFilter(Permission = "kep:device:remove")]
        [Log(Title = "设备删除", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string id)
        {
            long[] ids = Tools.SpitLongArrary(id);
            return ToResponse(DeviceService.DeleteDeviceByIds(ids));
        }

        /// <summary>
        /// 设备导出
        /// </summary>
        [Log(BusinessType = BusinessType.EXPORT, IsSaveResponseData = false, Title = "设备导出")]
        [HttpGet()]
        [ActionPermissionFilter(Permission = "kep:device:export")]
        public IActionResult Export()
        {
            var list = DeviceService.SelectDeviceAll();
            var result = ExportExcelMini(list, "device", "设备列表");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}
