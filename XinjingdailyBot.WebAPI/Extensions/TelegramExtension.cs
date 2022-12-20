using Microsoft.Extensions.Options;
using System.Net;
using Telegram.Bot;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Interface.Bot;
using XinjingdailyBot.Service.Bot;

namespace XinjingdailyBot.WebAPI.Extensions
{
    public static class TelegramExtension
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public static void AddTelegramBotClient(this IServiceCollection services)
        {
            services.AddSingleton<ITelegramBotClient>(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<OptionsSetting>>().Value;

                string? proxy = config.Bot.Proxy;

                HttpClient? httpClient = null;
                if (!string.IsNullOrEmpty(proxy))
                {
                    httpClient = new(
                        new HttpClientHandler()
                        {
                            Proxy = new WebProxy() { Address = new Uri(proxy) },
                            UseProxy = true,
                        }
                    );
                }

                string? token = config.Bot.BotToken;

                if (string.IsNullOrEmpty(token))
                {
                    _logger.Error("Telegram bot token ²»ÄÜÎª¿Õ");
                    Environment.Exit(1);
                }

                TelegramBotClientOptions options = new(token);
                return new TelegramBotClient(options, httpClient);
            });

            services.AddHostedService<PollingService>();
        }

    }

}
