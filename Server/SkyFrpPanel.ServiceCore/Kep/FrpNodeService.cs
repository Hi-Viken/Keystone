using SqlSugar;
using System.Security.Cryptography;
using SkyFrpPanel.Infrastructure;
using SkyFrpPanel.Infrastructure.Attribute;
using SkyFrpPanel.Model;
using SkyFrpPanel.Model.System;
using SkyFrpPanel.Repository;
using SkyFrpPanel.Model.Kep.Model;
using SkyFrpPanel.Model.Kep.Dto;

namespace SkyFrpPanel.ServiceCore.Kep
{
    /// <summary>
    /// 节点管理
    /// </summary>
    [AppService(ServiceType = typeof(IFrpNodeService), ServiceLifetime = LifeTime.Transient)]
    public class FrpNodeService : BaseService<FrpNode>, IFrpNodeService
    {
        private const string MASK_TOKEN = "********";

        /// <summary>
        /// 查询节点列表（分页），令牌默认脱敏
        /// </summary>
        public PagedInfo<FrpNode> SelectNodeList(FrpNodeQueryDto dto)
        {
            var predicate = QueryExp(dto);
            var result = Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(it => it.NodeId)
                .ToPage(dto);

            foreach (var item in result.Result)
            {
                item.Tocken = string.IsNullOrEmpty(item.Tocken) ? "" : MASK_TOKEN;
            }
            return result;
        }

        /// <summary>
        /// 根据节点编号获取详细信息，令牌脱敏
        /// </summary>
        public FrpNode SelectNodeById(long nodeId)
        {
            var node = GetId(nodeId);
            if (node != null)
            {
                node.Tocken = string.IsNullOrEmpty(node.Tocken) ? "" : MASK_TOKEN;
            }
            return node;
        }

        /// <summary>
        /// 新增节点
        /// </summary>
        public long InsertNode(FrpNode node)
        {
            if (UserConstants.NOT_UNIQUE.Equals(CheckNodeNameUnique(node)))
                throw new CustomException($"添加节点'{node.NodeName}'失败，节点名称已存在");

            return InsertReturnBigIdentity(node);
        }

        /// <summary>
        /// 修改节点
        /// </summary>
        public int UpdateNode(FrpNode node)
        {
            if (UserConstants.NOT_UNIQUE.Equals(CheckNodeNameUnique(node)))
                throw new CustomException($"修改节点'{node.NodeName}'失败，节点名称已存在");

            // 令牌为空时不更新令牌字段（前端编辑时可留空表示不修改）
            if (string.IsNullOrEmpty(node.Tocken))
            {
                return Context.Updateable(node)
                    .IgnoreColumns(it => it.Tocken)
                    .ExecuteCommand();
            }

            return Update(node);
        }

        /// <summary>
        /// 批量删除节点
        /// </summary>
        public int DeleteNodeByIds(long[] nodeIds)
        {
            return Delete(nodeIds);
        }

        /// <summary>
        /// 修改节点状态
        /// </summary>
        public int UpdateNodeStatus(FrpNode node)
        {
            return Context.Updateable(node)
                .UpdateColumns(it => new { it.IsEnable, it.Update_by, it.Update_time })
                .ExecuteCommand();
        }

        /// <summary>
        /// 校验节点名称是否唯一
        /// </summary>
        public string CheckNodeNameUnique(FrpNode node)
        {
            FrpNode info = GetFirst(it => it.NodeName.Equals(node.NodeName));
            if (info != null && info.NodeId != node.NodeId)
                return UserConstants.NOT_UNIQUE;
            return UserConstants.UNIQUE;
        }

        /// <summary>
        /// 获取所有节点（导出用）
        /// </summary>
        public List<FrpNode> SelectNodeAll()
        {
            return Queryable().OrderByDescending(it => it.NodeId).ToList();
        }

        /// <summary>
        /// 生成唯一令牌（32位hex，不与已有重复）
        /// </summary>
        public string GenerateToken()
        {
            string token;
            int maxRetry = 10;
            do
            {
                byte[] bytes = new byte[16];
                RandomNumberGenerator.Fill(bytes);
                token = Convert.ToHexString(bytes).ToLower();
                maxRetry--;
            }
            while (Count(it => it.Tocken == token) > 0 && maxRetry > 0);

            return token;
        }

        /// <summary>
        /// 根据节点编号获取令牌明文
        /// </summary>
        public string SelectTokenById(long nodeId)
        {
            var node = GetId(nodeId);
            return node?.Tocken ?? "";
        }

        /// <summary>
        /// 构建查询条件
        /// </summary>
        private static Expressionable<FrpNode> QueryExp(FrpNodeQueryDto dto)
        {
            var predicate = Expressionable.Create<FrpNode>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.NodeName), it => it.NodeName.Contains(dto.NodeName));
            predicate = predicate.AndIF(dto.IsEnable != null, it => it.IsEnable == dto.IsEnable);
            return predicate;
        }
    }
}
