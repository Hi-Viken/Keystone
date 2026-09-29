using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Keystone.Infrastructure
{
    /// <summary>
    /// 跨域扩展
    /// </summary>
    public static class CorsExtension
    {
        /// <summary>
        /// 跨域配置
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddCors(this IServiceCollection services, IConfiguration configuration)
        {
            var corsUrls = configuration.GetSection("corsUrls").Get<string[]>();

            //配置跨域
            services.AddCors(c =>
            {
                c.AddPolicy("Policy", policy =>
                {
                    policy.WithOrigins(corsUrls ?? Array.Empty<string>())
                    .AllowAnyHeader()//允许任意头
                    .AllowCredentials()//允许cookie
                    .AllowAnyMethod()//允许任意方法
                    .WithExposedHeaders(
                        "Content-Disposition",
                        "Content-Encoding",
                        "Content-Range",
                        "Date",
                        "Server",
                        "Transfer-Encoding",
                        "ETag",
                        "Last-Modified",
                        "Vary",
                        "X-Total-Count",
                        "X-Page-Index",
                        "X-Page-Size",
                        "X-Request-Id",
                        "X-Powered-By",
                        "Authorization",
                        "WWW-Authenticate",
                        "Location",
                        "Retry-After",
                        "Set-Cookie",
                        "Access-Control-Expose-Headers"
                    );//暴露响应头给前端（CORS默认只暴露6个安全头）
                });
            });
        }
    }
}
