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
                var initServices = services.GetServices<IInitializeService>()
                    .OrderBy(static service => service.Order)
                    .ToList();

                if (initServices.Count == 0)
                {
                    _logger.Warn("未找到任何初始化服务");
                    return;
                }

                _logger.Info(Langs.Line);

                foreach (var initService in initServices)
                {
                    var serviceName = initService.Name;

                    _logger.Info("开始初始化服务：{ServiceName}", serviceName);

                    await initService.InitializeAsync().ConfigureAwait(false);

                    _logger.Info("初始化服务 {ServiceName} 成功", serviceName);
                }

                _logger.Info(Langs.Line);
                _logger.Info("初始化完成");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "初始化服务时发生未预期的异常");
                throw;
            }
        }
    }
}