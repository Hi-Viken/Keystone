using SkyFrpPanel.Model.System;

namespace SkyFrpPanel.Model.Kep.Model
{
    /// <summary>
    /// 节点表
    /// </summary>
    [SugarTable("FrpNode", "节点表")]
    [Tenant("0")]
    public class FrpNode : SysBase
    {
        /// <summary>
        /// 用户id
        /// </summary>
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true,ColumnName = "NodeId")]
        public long NodeId { get; set; }
        /// <summary>
        /// 登录用户名
        /// </summary>
        [SugarColumn(Length = 30, ColumnDescription = "节点名称", ExtendedAttribute = ProteryConstant.NOTNULL,ColumnName = "NodeName")]
        public string NodeName { get; set; }

        [SugarColumn(Length = 32, ColumnDescription = "令牌", ExtendedAttribute = ProteryConstant.NOTNULL, ColumnName = "Tocken")]
        public string Tocken { get; set; }


        [SugarColumn(Length = 1, ColumnDescription = "是否启用", ExtendedAttribute = ProteryConstant.NOTNULL, ColumnName = "IsEnable")]
        public bool IsEnable { get; set; }
    }
}
