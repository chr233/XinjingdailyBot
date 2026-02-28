using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using Telegram.Bot;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Service.HostedService;

namespace XinjingDaily.Bot.WebAPI.Extensions;

/// <summary>
/// Telegram扩展
/// </summary>
public static class TelegramExtension
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private const string TelegramClient = "telegram_bot_client";

    extension(IServiceCollection services)
    {
        /// <summary>
        /// 注册Telegram客户端
        /// </summary>
        /// <param name="services"></param>
        public void AddTelegramBotClient()
        {
            string publicIdentifier = $"{BuildInfo.AppName}-{BuildInfo.Variant}";

            services.AddHttpClient(TelegramClient, (serviceProvider, httpClient) => {
                var config = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value.Network;
                httpClient.BaseAddress = HttpClientExtension.GetBaseAddress(TelegramClient, config.TelegramApi);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(publicIdentifier, BuildInfo.Version));
            }).ConfigurePrimaryHttpMessageHandler(serviceProvider => {
                var config = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value.Bot;
                string? proxy = config.BotProxy;
                return HttpClientExtension.CreateHttpClientHandler(TelegramClient, proxy);
            }).AddTypedClient<ITelegramBotClient>((httpClient, sp) => {
                var config = sp.GetRequiredService<IOptions<AppSettings>>().Value.Bot;

                if (string.IsNullOrEmpty(config.BotToken))
                {
                    _logger.Error("BotToken 不能为空, 请检查 Bot 节");
                    _logger.Error("按任意键退出...");
                    Console.ReadKey();
                    Environment.Exit(1);
                }

                TelegramBotClientOptions options = new(config.BotToken);
                return new TelegramBotClient(options, httpClient);
            }).RemoveAllLoggers();

            services.AddHostedService<PollingService>();

            //services.AddHostedService<StatisticService>();

            //services.AddHostedService<BotInitializationServices>();
        }
    }
}
