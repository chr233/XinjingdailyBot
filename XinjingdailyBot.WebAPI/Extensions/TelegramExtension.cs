namespace XinjingdailyBot.WebAPI.Extensions;

/// <summary>
/// Telegram扩展
/// </summary>
public static class TelegramExtension
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    /// <summary>
    /// 注册Telegram客户端
    /// </summary>
    /// <param name="services"></param>
    public static void AddTelegramBotClient(this IServiceCollection services)
    {
        //services.AddSingleton<ITelegramBotClient>(serviceProvider => {
        //    var httpHelperService = serviceProvider.GetRequiredService<IHttpHelperService>();
        //    var httpClient = httpHelperService.CreateClient("Telegram");

        //    var config = serviceProvider.GetRequiredService<IOptions<OptionsSetting>>().Value;
        //    string? token = config.Bot.BotToken;

        //    if (string.IsNullOrEmpty(token))
        //    {
        //        _logger.Error("Telegram bot token 不能为空");
        //        _logger.Error("按任意键退出...");
        //        Console.ReadKey();
        //        Environment.Exit(1);
        //    }

        //    string? baseUrl = config.Bot.BaseUrl;

        //    var options = new TelegramBotClientOptions(token, baseUrl, false);
        //    return new TelegramBotClient(options, httpClient);
        //});

        //services.AddHostedService<PollingService>();
        //services.AddHostedService<StatisticService>();
    }
}
