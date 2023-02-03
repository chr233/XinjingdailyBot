using Microsoft.Extensions.Options;
using Telegram.Bot;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Service.Bot.Common;
using XinjingdailyBot.Tasks;

namespace XinjingdailyBot.WebAPI.Extensions
{
    public static class TelegramExtension
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public static void AddTelegramBotClient(this IServiceCollection services)
        {
            services.AddSingleton<ITelegramBotClient>(serviceProvider =>
            {
                var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("Telegram");

                var config = serviceProvider.GetRequiredService<IOptions<OptionsSetting>>().Value;
                string? token = config.Bot.BotToken;

                if (string.IsNullOrEmpty(token))
                {
                    _logger.Error("Telegram bot token 不能为空");
                    Environment.Exit(1);
                }

                string? baseUrl = config.Bot.BaseUrl;

                TelegramBotClientOptions options = new(token, baseUrl, false);
                return new TelegramBotClient(options, httpClient);
            });

            services.AddHostedService<PollingService>();
            services.AddHostedService<ExpiredPostsTask>();
            services.AddHostedService<RejectChannelTask>();
        }
    }
}
