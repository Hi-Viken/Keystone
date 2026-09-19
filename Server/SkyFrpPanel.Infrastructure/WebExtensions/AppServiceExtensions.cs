using SkyFrpPanel.Infrastructure.Attribute;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SkyFrpPanel.Infrastructure
{
    /// <summary>
    /// App服务注册
    /// </summary>
    public static class AppServiceExtensions
    {
        /// <summary>
        /// 需要在启动时立即实例化的服务类型集合
        /// </summary>
        private static readonly List<Type> _eagerInitServiceTypes = new();

        /// <summary>
        /// 注册引用程序域中所有有AppService标记的类的服务
        /// </summary>
        /// <param name="services"></param>
        public static void AddAppService(this IServiceCollection services)
        {
            var cls = AppSettings.Get<string[]>("InjectClass");
            if (cls == null || cls.Length <= 0)
            {
                throw new Exception("请更新appsettings类");
            }
            foreach (var item in cls)
            {
                Register(services, item);
            }
        }

        /// <summary>
        /// 在应用启动时立即实例化所有标记了 EagerInit = true 的单例服务
        /// 需在 app = builder.Build() 之后调用
        /// </summary>
        /// <param name="serviceProvider"></param>
        public static void InitEagerServices(this IServiceProvider serviceProvider)
        {
            foreach (var serviceType in _eagerInitServiceTypes)
            {
                serviceProvider.GetRequiredService(serviceType);
            }
        }

        private static void Register(IServiceCollection services, string item)
        {
            Assembly assembly = Assembly.Load(item);
            foreach (var type in assembly.GetTypes())
            {
                var serviceAttribute = type.GetCustomAttribute<AppServiceAttribute>();

                if (serviceAttribute != null)
                {
                    var serviceType = serviceAttribute.ServiceType;
                    //情况1 适用于依赖抽象编程，注意这里只获取第一个
                    if (serviceType == null && serviceAttribute.InterfaceServiceType)
                    {
                        serviceType = type.GetInterfaces().FirstOrDefault();
                    }
                    //情况2 不常见特殊情况下才会指定ServiceType，写起来麻烦
                    if (serviceType == null)
                    {
                        serviceType = type;
                    }

                    switch (serviceAttribute.ServiceLifetime)
                    {
                        case LifeTime.Singleton:
                            services.AddSingleton(serviceType, type);
                            // 收集需要立即初始化的单例服务
                            if (serviceAttribute.EagerInit)
                            {
                                _eagerInitServiceTypes.Add(serviceType);
                            }
                            break;
                        case LifeTime.Scoped:
                            services.AddScoped(serviceType, type);
                            break;
                        case LifeTime.Transient:
                            services.AddTransient(serviceType, type);
                            break;
                        default:
                            services.AddTransient(serviceType, type);
                            break;
                    }
                    //System.Console.WriteLine($"注册：{serviceType}");
                }
            }
        }
    }
}
