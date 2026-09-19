using SkyFrpPanel.Model;
using SkyFrpPanel.Model.Kep.Dto;
using SkyFrpPanel.Model.Kep.Model;

namespace SkyFrpPanel.ServiceCore.Kep
{
    public interface IFrpNodeService : IBaseService<FrpNode>
    {
        /// <summary>
        /// 查询节点列表（分页）
        /// </summary>
        PagedInfo<FrpNode> SelectNodeList(FrpNodeQueryDto dto);

        /// <summary>
        /// 根据节点编号获取详细信息
        /// </summary>
        FrpNode SelectNodeById(long nodeId);

        /// <summary>
        /// 新增节点
        /// </summary>
        long InsertNode(FrpNode node);

        /// <summary>
        /// 修改节点
        /// </summary>
        int UpdateNode(FrpNode node);

        /// <summary>
        /// 批量删除节点
        /// </summary>
        int DeleteNodeByIds(long[] nodeIds);

        /// <summary>
        /// 修改节点状态
        /// </summary>
        int UpdateNodeStatus(FrpNode node);

        /// <summary>
        /// 校验节点名称是否唯一
        /// </summary>
        string CheckNodeNameUnique(FrpNode node);

        /// <summary>
        /// 获取所有节点（导出用）
        /// </summary>
        List<FrpNode> SelectNodeAll();

        /// <summary>
        /// 生成唯一令牌（不与已有重复）
        /// </summary>
        string GenerateToken();

        /// <summary>
        /// 根据节点编号获取令牌明文
        /// </summary>
        string SelectTokenById(long nodeId);
    }
}
