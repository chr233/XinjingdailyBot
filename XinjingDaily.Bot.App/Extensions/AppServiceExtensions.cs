namespace XinjingDaily.Bot.WebAPI.Extensions;

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
}