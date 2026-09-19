using System;
using System.Collections.Generic;
using System.Text;

namespace SkyFrpPanel.Model.System.ServerInfo
{

    /// <summary>
    /// 服务器基础信息
    /// </summary>
    public class ServerSimpleInfoDto
    {
        /// <summary>
        /// 主机名
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// Ip列表
        /// </summary>
        public List<string> IpList { get; set; }
        /// <summary>
        /// DNS服务器列表
        /// </summary>
        public List<string> DnsInfoList { get; set; }
        /// <summary>
        /// 网关列表
        /// </summary>
        public List<string> GatewayList { get; set; }
        /// <summary>
        /// 系统名称
        /// </summary>
        public string OsName { get; set; }
        /// <summary>
        /// 系统版本
        /// </summary>
        public string OsVersion { get; set; }
        /// <summary>
        /// 内核版本
        /// </summary>
        public string KernelVersion { get; set; }
        /// <summary>
        /// 启动模式
        /// </summary>
        public string BootMode { get; set; }
        /// <summary>
        /// 系统启动时常
        /// </summary>
        public TimeSpan SystemRunTime { get; set; }
    }

    public class ServerInfDto
    {
        
    }
}
