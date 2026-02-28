using XinjingDaily.Bot.Interface.InitService;
using XinjingDaily.Bot.Service.InitService;

namespace XinjingDaily.Bot.App.Extensions;

/// <summary>
/// 动态注册服务扩展
/// </summary>
public static class AppServiceExtensions
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    extension(IServiceCollection services)
    {
        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="services"></param>
        public void AddAppService()
        {
            services.AddXinjingDailyBotRepository();
            services.AddXinjingDailyBotService();
            services.AddXinjingDailyBotCommand();

            services.AddTransient<IInitializeService, DbInitializeService>();
            services.AddTransient<IInitializeService, RedisInitializeService>();
            services.AddTransient<IInitializeService, BotInitializeService>();
        }
    }

    extension(WebApplication app)
    {
        public async Task TryInitializeServicesAsync()
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                // 1. 获取所有IInitializeService实现类，并按Order从小到大排序
                var initServices = services.GetServices<IInitializeService>()
                                          .OrderBy(service => service.Order)
                                          .ToList();

                _logger.Info($"共找到 {initServices.Count} 个初始化服务"); // 应该输出 3

                if (initServices.Count == 0)
                {
                    _logger.Warn("未找到任何实现IInitializeService的初始化服务");
                    return;
                }

                // 2. 按排序后的顺序依次执行初始化
                foreach (var initService in initServices)
                {
                    var serviceName = initService.GetType().Name;
                    _logger.Debug("初始化服务：{ServiceName}", serviceName);

                    var success = await initService.InitializeAsync(CancellationToken.None).ConfigureAwait(false);

                    // 3. 初始化失败则抛出异常
                    if (!success)
                    {
                        var errorMsg = $"初始化服务 {serviceName} 失败";
                        _logger.Error(errorMsg);
                        throw new InvalidOperationException(errorMsg);
                    }

                    _logger.Info("初始化服务 {ServiceName} 成功", serviceName);
                }

                _logger.Info("所有初始化服务已按顺序执行完成");
            }
            catch (InvalidOperationException)
            {
                // 主动抛出的初始化失败异常，直接向上传递
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "初始化服务时发生未预期的异常");
                // 封装异常后抛出，方便上层统一处理
                throw new ApplicationException("初始化服务过程中出现未预期错误", ex);
            }

        }
    }
}