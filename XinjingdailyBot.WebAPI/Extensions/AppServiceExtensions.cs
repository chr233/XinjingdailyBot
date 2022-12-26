﻿using System.Reflection;
using System.Text;
using XinjingdailyBot.Infrastructure.Attribute;

namespace XinjingdailyBot.WebAPI.Extensions
{
    public static class AppServiceExtensions
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 注册引用程序域中所有有AppService标记的类的服务
        /// </summary>
        /// <param name="services"></param>
        public static void AddAppService(this IServiceCollection services)
        {
            string[] cls = new string[] { "XinjingdailyBot.Repository", "XinjingdailyBot.Service", "XinjingdailyBot.Command" };
            foreach (var item in cls)
            {
                Assembly assembly = Assembly.Load(item);
                Register(services, assembly);
            }
        }

        private static void Register(IServiceCollection services, Assembly assembly)
        {
            StringBuilder sb = new();
            sb.AppendLine();
            sb.AppendLine("===== 开始注册服务 =====");
            int count = 0;
            foreach (var type in assembly.GetTypes())
            {
                var serviceAttribute = type.GetCustomAttribute<AppServiceAttribute>();
                if (serviceAttribute != null)
                {
                    count += 1;
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

                    var lifetime = serviceAttribute.ServiceLifetime;
                    switch (lifetime)
                    {
                        case LifeTime.Singleton:
                            services.AddSingleton(serviceType, type);
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

                    sb.AppendLine($"[{lifetime}]：{serviceType}");
                }
            }
            sb.Append($"===== 注册了 {count} 个服务 =====");

            _logger.Debug(sb.ToString());
        }
    }
}
