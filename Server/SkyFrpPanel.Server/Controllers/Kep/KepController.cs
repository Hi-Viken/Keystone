using Microsoft.AspNetCore.Mvc;
using SkyFrpPanel.Model.Kep.Dto;
using SkyFrpPanel.Model.Kep.Model;
using SkyFrpPanel.ServiceCore.Kep;
using SkyFrpPanel.ServiceCore.Kep.IService;

namespace SkyFrpPanel.Server.Controllers.Kep
{
    /// <summary>
    /// Kep服务
    /// </summary>
    [Route("[controller]/[action]")]
    //[ApiExplorerSettings(GroupName = "sys")]
    public class KepController:BaseController
    {
        private readonly IKepServerManager server;
        private readonly IFrpNodeService NodeService;
        public KepController(IKepServerManager serverManager,IFrpNodeService nodeService) {
            server=serverManager;
            NodeService = nodeService;
        }




        [HttpGet]
        public IActionResult GetServerStatus()
        {
            var result =server.GetServerStatus();
            return SUCCESS(result);
        }

        /// <summary>
        /// 节点列表查询
        /// </summary>
        [HttpGet]
        [ActionPermissionFilter(Permission = "frp:node:list")]
        public IActionResult List([FromQuery] FrpNodeQueryDto dto)
        {
            var list = NodeService.SelectNodeList(dto);
            return SUCCESS(list);
        }

        /// <summary>
        /// 根据节点编号获取详细信息
        /// </summary>
        [HttpGet("{nodeId}")]
        [ActionPermissionFilter(Permission = "frp:node:query")]
        public IActionResult Query(long nodeId = 0)
        {
            return SUCCESS(NodeService.SelectNodeById(nodeId));
        }

        /// <summary>
        /// 生成唯一令牌
        /// </summary>
        [HttpGet()]
        [ActionPermissionFilter(Permission = "frp:node:edit")]
        public IActionResult GenerateToken()
        {
            return SUCCESS(new { token = NodeService.GenerateToken() });
        }

        /// <summary>
        /// 根据节点编号获取令牌明文
        /// </summary>
        [HttpGet("{nodeId}")]
        [ActionPermissionFilter(Permission = "frp:node:query")]
        public IActionResult GetToken(long nodeId)
        {
            return SUCCESS(new { token = NodeService.SelectTokenById(nodeId) });
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "frp:node:add")]
        [Log(Title = "节点添加", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] FrpNode node)
        {
            node.ToCreate(HttpContext);
            return ToResponse(NodeService.InsertNode(node));
        }

        /// <summary>
        /// 修改节点
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "frp:node:edit")]
        [Log(Title = "节点编辑", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] FrpNode node)
        {
            node.ToUpdate(HttpContext);
            return ToResponse(NodeService.UpdateNode(node));
        }

        /// <summary>
        /// 节点删除
        /// </summary>
        [HttpDelete("{id}")]
        [ActionPermissionFilter(Permission = "frp:node:remove")]
        [Log(Title = "节点删除", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string id)
        {
            long[] ids = Tools.SpitLongArrary(id);
            return ToResponse(NodeService.DeleteNodeByIds(ids));
        }

        /// <summary>
        /// 修改节点状态
        /// </summary>
        [HttpPut()]
        [ActionPermissionFilter(Permission = "frp:node:edit")]
        [Log(Title = "修改节点状态", BusinessType = BusinessType.UPDATE)]
        public IActionResult ChangeStatus([FromBody] FrpNode node)
        {
            var updateObj = new FrpNode()
            {
                NodeId = node.NodeId,
                IsEnable = node.IsEnable
            };
            updateObj.ToUpdate(HttpContext);
            return ToResponse(NodeService.UpdateNodeStatus(updateObj));
        }

        /// <summary>
        /// 节点导出
        /// </summary>
        [Log(BusinessType = BusinessType.EXPORT, IsSaveResponseData = false, Title = "节点导出")]
        [HttpGet()]
        [ActionPermissionFilter(Permission = "frp:node:export")]
        public IActionResult Export()
        {
            var list = NodeService.SelectNodeAll();
            var result = ExportExcelMini(list, "node", "节点列表");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}



