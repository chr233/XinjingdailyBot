using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using XinjingdailyBot.Infrastructure.Attribute;

namespace XinjingdailyBot.WebAPI.Extensions
{
    /// <summary>
    /// 动态注册服务扩展
    /// </summary>
    public static class AppServiceExtensions
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 注册引用程序域中所有有AppService标记的类的服务
        /// </summary>
        /// <param name="services"></param>
        [RequiresUnreferencedCode("不兼容剪裁")]
        public static void AddAppService(this IServiceCollection services)
        {
            string[] cls = new[] { "XinjingdailyBot.Repository", "XinjingdailyBot.Service", "XinjingdailyBot.Command" };
            foreach (var item in cls)
            {
                Assembly assembly = Assembly.Load(item);
                Register(services, assembly);
            }
        }

        /// <summary>
        /// 动态注册服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly"></param>
        [RequiresUnreferencedCode("不兼容剪裁")]
        private static void Register(IServiceCollection services, Assembly assembly)
        {
            string? name = assembly.GetName().Name;
            _logger.Debug($"===== 注册 {name} 中的服务 =====");
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

                    _logger.Debug($"{lifetime} - {serviceType}");
                }
            }
            _logger.Debug($"===== 注册了 {count} 个服务 =====");
        }
    }
}
