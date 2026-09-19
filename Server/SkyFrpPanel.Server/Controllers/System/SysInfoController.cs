using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SkyFrpPanel.Controllers.System
{
    /// <summary>
    /// 获取系统信息水水水水
    /// </summary>
    [Route("api/[controller]")]
    //[ApiExplorerSettings(GroupName = "sys")]
    //[AllowAnonymous]
    public class SysInfoController :  BaseController
    {
        private readonly ISysConfigService _SysConfigService;

        public SysInfoController(ISysConfigService SysConfigService)
        {
            _SysConfigService = SysConfigService;
        }

        /// <summary>
        /// 获取系统类型
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetSysType")]
        public IActionResult GetSysType()
        {
            ResultTrans<object> result = new ResultTrans<object>();
            result.Data = new { SystemType = "Linux" };
            result.Success = true;
            result.Code= ApiCode.Success;
            return Json(result);
        }
    }
}
