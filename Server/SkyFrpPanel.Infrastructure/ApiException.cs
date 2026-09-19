using SkyFrpPanel.Infrastructure;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyFrpPanel.Infrastructure
{

    public class ApiException : Exception
    {
        public int StatusCode { get; }   // HTTP 状态码
        public int Code { get; }         // 业务码

        public ApiException(string message, int statusCode, int code)
            : base(message)
        {
            StatusCode = statusCode;
            Code = code;
        }
    }


    //public  class FuException : Exception
    //{
    //    public int Code { get; set; }
    //    public bool Success = false;
    //    public string Msg { get; set; }

    //    public FuException(int Code,string message) : base(message) { 
    //        this.Code = Code;
    //        this.Msg = message;
    //    }
    //}
 }
