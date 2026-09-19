using System;
using System.Collections.Generic;
using System.Text;

namespace SkyFrpPanel.Model.ControlHub.Dto
{
    /// <summary>
    /// 主机概述
    /// </summary>
    public class HostInfoVo
    {
        /// <summary>
        /// 主机名
        /// </summary>
        public string Hostname { get; set; }
        /// <summary>
        /// IP
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// CPU名称型号
        /// </summary>
        public string Cpu { get; set; }
        /// <summary>
        /// 运行内存
        /// </summary>
        public ulong RAM { get; set; }
        /// <summary>
        /// 硬盘
        /// </summary>
        public List<DisksVo> Disks { get; set; }
        /// <summary>
        /// 系统
        /// </summary>
        public OsVo OS { get; set; }
    }

    /// <summary>
    /// 硬盘
    /// </summary>
    public class DisksVo
    {
       
        /// <summary>
        /// 硬盘名
        /// </summary>
        public string Device { get; set; }
        /// <summary>
        /// 硬盘大小（单位byte    size / 1024 / 1024 / 1024）
        /// </summary>
        public ulong Size { get; set; }
    }

    public class OsVo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Kernel { get; set; }
        public string BootMode { get; set; }
    }

}
