using System;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SkyFrpPanel.Infrastructure
{
    /// <summary>
    /// 接口通用响应结果（带数据 + 支持提示语模板+动态参数）
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class ResultTrans<T>
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// 业务状态码（数字/十六进制字符串，如 "200"、"0x1000"）
        /// </summary>
        public int Code { get; set; } = ApiCode.Success;

        /// <summary>
        /// 提示语模板（支持占位符：{0}/{1} 或 {参数名}）
        /// 示例："用户{0}的订单{1}不存在" 或 "用户{userId}的订单{orderNo}不存在"
        /// </summary>
        public string MsgTemplate { get; set; } = "Operation completed successfully.";

        /// <summary>
        /// 提示语模板参数（数组/字典，匹配占位符）
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object[] MsgParams { get; set; }

        /// <summary>
        /// 最终渲染后的提示语（前端优先使用此字段）
        /// </summary>
        public string Msg => RenderMsg();

        /// <summary>
        /// 响应数据
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T Data { get; set; }

        // 构造函数
        public ResultTrans() { }

        public ResultTrans(bool success, int code, string msgTemplate, T data, params object[] msgParams)
        {
            Success = success;
            Code = code;
            MsgTemplate = msgTemplate;
            MsgParams = msgParams;
            Data = data;
        }

        public ResultTrans(bool success, int code, string msgTemplate, params object[] msgParams)
        {
            Success = success;
            Code = code;
            MsgTemplate = msgTemplate;
            MsgParams = msgParams;
        }

        /// <summary>
        /// 渲染提示语（替换占位符）
        /// 支持两种占位符：
        /// 1. 索引占位符：{0}、{1}（匹配MsgParams数组索引）
        /// 2. 命名占位符：{userId}（需MsgParams为字典类型，暂简化为索引优先）
        /// </summary>
        private string RenderMsg()
        {
            if (MsgParams == null || MsgParams.Length == 0 || string.IsNullOrEmpty(MsgTemplate))
            {
                return MsgTemplate ?? string.Empty;
            }

            try
            {
                // 方案1：索引占位符（推荐，简单通用）
                return string.Format(MsgTemplate, MsgParams);

                // 可选：方案2 - 支持命名占位符（如 {userId}），需MsgParams为Dictionary<string, object>
                // if (MsgParams.FirstOrDefault() is Dictionary<string, object> namedParams)
                // {
                //     var result = MsgTemplate;
                //     foreach (var param in namedParams)
                //     {
                //         result = Regex.Replace(result, $"\\{{{param.Key}\\}}", param.Value?.ToString() ?? string.Empty);
                //     }
                //     return result;
                // }
                // return string.Format(MsgTemplate, MsgParams);
            }
            catch (Exception)
            {
                // 渲染失败时返回原始模板（避免接口报错）
                return MsgTemplate;
            }
        }

        // 静态工厂方法
        /// <summary>
        /// 成功结果（支持模板+参数）
        /// </summary>
        public static ResultTrans<T> SuccessResult(T data, string msgTemplate = "Operation completed successfully.", params object[] msgParams)
        {
            return new ResultTrans<T>(true, ApiCode.Success, msgTemplate, data, msgParams);
        }

        /// <summary>
        /// 失败结果（支持模板+参数）
        /// </summary>
        public static ResultTrans<T> FailResult(int code, string msgTemplate = "Operation failed.", params object[] msgParams)
        {
            return new ResultTrans<T>(false, code, msgTemplate, msgParams);
        }
    }

    /// <summary>
    /// 接口通用响应结果（无数据 + 支持提示语模板+动态参数）
    /// </summary>
    public class ResultTrans
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// 业务状态码（数字/十六进制字符串）
        /// </summary>
        public int Code { get; set; } = ApiCode.Success;

        /// <summary>
        /// 提示语模板（支持{0}/{1}占位符）
        /// </summary>
        public string MsgTemplate { get; set; } = "Operation completed successfully.";

        /// <summary>
        /// 提示语模板参数
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object[] MsgParams { get; set; }

        /// <summary>
        /// 最终渲染后的提示语
        /// </summary>
        public string Msg => RenderMsg();

        // 构造函数
        public ResultTrans() { }

        public ResultTrans(bool success, int code, string msgTemplate, params object[] msgParams)
        {
            Success = success;
            Code = code;
            MsgTemplate = msgTemplate;
            MsgParams = msgParams;
        }

        /// <summary>
        /// 渲染提示语
        /// </summary>
        private string RenderMsg()
        {
            if (MsgParams == null || MsgParams.Length == 0 || string.IsNullOrEmpty(MsgTemplate))
            {
                return MsgTemplate ?? string.Empty;
            }

            try
            {
                return string.Format(MsgTemplate, MsgParams);
            }
            catch (Exception)
            {
                return MsgTemplate;
            }
        }

        // 静态工厂方法
        public static ResultTrans SuccessResult(string msgTemplate = "Operation completed successfully.", params object[] msgParams)
        {
            return new ResultTrans(true, ApiCode.Success, msgTemplate, msgParams);
        }

        public static ResultTrans FailResult(int code, string msgTemplate = "Operation failed.", params object[] msgParams)
        {
            return new ResultTrans(false, code, msgTemplate, msgParams);
        }
    }

    /// <summary>
    /// 业务状态码常量
    /// </summary>
    public static class ApiCode
    {
        // 基础通用码
        public const int Success = 200;               // 成功
        public const int Failure = 500;               // 通用失败
        public const int InvalidParam = 400;          // 参数无效
        public const int Unauthorized = 401;          // 未授权
        public const int Forbidden = 403;             // 权限不足
        public const int NotFound = 404;              // 资源不存在

        // 业务模块码
        public const string User_NotFound = "10001";       // 用户模块-用户不存在
        public const string User_PwdError = "10002";       // 用户模块-密码错误
        public const string Order_Closed = "20001";        // 订单模块-订单已关闭
        public const string Order_NotFound = "20002";      // 订单模块-订单不存在
    }
}