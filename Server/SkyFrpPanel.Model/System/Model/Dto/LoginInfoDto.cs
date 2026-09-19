using System;
using System.Collections.Generic;
using System.Text;

namespace SkyFrpPanel.Model.System.Model.Dto
{
    public class LoginInfoDto
    {
        /// <summary>
        /// Ip地址
        /// </summary>
        public string Ipaddr { get; set; }
        /// <summary>
        /// 登录时间
        /// </summary>
        public DateTime? LoginTime { get; set; }
        /// <summary>
        /// 登录位置
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// 登录状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 上次登录Id
        /// </summary>

        public string LastIpaddr { get; set; }
        /// <summary>
        /// 上次登录时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }
        /// <summary>
        /// 上次登录位置
        /// </summary>
        public string LastLocation { get; set; }
        /// <summary>
        /// 上次登录状态
        /// </summary>
        public string LastStatus { get; set; }
    }
}
