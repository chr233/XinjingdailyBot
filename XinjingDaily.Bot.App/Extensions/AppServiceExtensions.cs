using XinjingDaily.Bot.Interface.InitService;

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
                var initServices = services.GetServices<IServiceInitializer>()
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

                    _logger.Info("准备初始化【{ServiceName}】", serviceName);

                    await initService.InitializeAsync().ConfigureAwait(false);

                    _logger.Info("初始化成功");
                    _logger.Info(Langs.Line);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "初始化服务时发生未预期的异常");
                throw;
            }
        }
    }
}