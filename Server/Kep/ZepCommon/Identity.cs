using System;
using System.Collections.Generic;
using System.Text;

namespace ZepCommon
{
    public class Identity
    {

        public Identity()
        {
        }
        public Identity(string sn,string tocke)
        { 
            this.SN = sn;
            this.Token = tocke;
        }



        /// <summary>
        /// 身份标识
        /// </summary>
        public string SN { get; set; }
        /// <summary>
        /// 令牌
        /// </summary>
        public string Token { get; set; }
    }


    /// <summary>认证响应</summary>
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}
