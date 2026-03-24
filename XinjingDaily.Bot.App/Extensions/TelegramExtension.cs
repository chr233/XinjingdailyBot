using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using Telegram.Bot;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Infrastructure.Utils;
using XinjingDaily.Bot.Service.HostedService;

namespace XinjingDaily.Bot.App.Extensions;

/// <summary>
/// Telegram扩展
/// </summary>
public static class TelegramExtension
{
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

            services.TryAddTransient<ITelegramBotClient>(sp => {
                var appSettings = sp.GetRequiredService<IOptions<AppSettings>>().Value;

                // 校验 BotToken
                if (string.IsNullOrEmpty(appSettings.Bot.BotToken))
                {
                    var logger = sp.GetRequiredService<ILogger<Program>>();
                    logger.LogError("BotToken 不能为空, 请检查 Bot 配置");
                    SystemUtils.Shutdown();
                }

                var handler = HttpClientExtension.CreateHttpClientHandler(TelegramClient, appSettings.Bot.BotProxy);

                var timeout = Math.Max(30, appSettings.Network.Timeout); // 最小30秒超时

                var httpClient = new HttpClient(handler, disposeHandler: false) {
                    BaseAddress = HttpClientExtension.GetBaseAddress(TelegramClient, appSettings.Network.TelegramApi),
                    Timeout = TimeSpan.FromSeconds(timeout),
                };

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(publicIdentifier, BuildInfo.Version));

                var options = new TelegramBotClientOptions(appSettings.Bot.BotToken);
                return new TelegramBotClient(options, httpClient);
            });

            services.AddHostedService<PollingService>();
        }
    }
}
