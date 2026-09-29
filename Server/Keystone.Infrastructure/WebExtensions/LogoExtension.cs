using Keystone.Infrastructure.Helper;
using JinianNet.JNTemplate;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Keystone.Infrastructure
{
    public static class LogoExtension
    {
        public static void AddLogo(this IServiceCollection services)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            var contentTpl = JnHelper.ReadTemplate("", "logo.txt");
            var content = contentTpl?.Render();
            var url = AppSettings.GetConfig("urls");
            Console.WriteLine(content);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("🎉源码地址: https://github.com/Hi-Viken/Keystone");
            Console.WriteLine("📖官方文档：http://www.vkin.cc");
            Console.WriteLine("💰打赏作者：http://www.vkin.cc");
            Console.WriteLine("📱移动端体验：http://demo.vkin.cc/h5");
            Console.WriteLine($"scalar地址：{url}/scalar");
            Console.WriteLine($"初始化种子数据地址：{url}/common/InitSeedData");
        }
    }
}
