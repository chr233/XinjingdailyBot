using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using XinjingdailyBot.Infrastructure;

namespace XinjingdailyBot.WebAPI.Extensions
{
    public static class HttpClientExtension
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public static void AddHttpClients(this IServiceCollection services)
        {
            services.AddHttpClient("Telegram", (serviceProvider, httpClient) =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<OptionsSetting>>().Value;
                string? baseUrl = config.Bot.BaseUrl;
                httpClient.BaseAddress = new Uri(baseUrl ?? "https://api.telegram.org/");
                httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
                httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, nameof(XinjingdailyBot));
            }).ConfigurePrimaryHttpMessageHandler(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<OptionsSetting>>().Value;
                string? proxy = config.Bot.Proxy;

                WebProxy? tgProxy = null;
                if (!string.IsNullOrEmpty(proxy))
                {
                    _logger.Info("已配置代理: {0}", proxy);
                    tgProxy = new WebProxy { Address = new Uri(proxy) };
                }
                return new HttpClientHandler
                {
                    Proxy = tgProxy,
                    UseProxy = tgProxy != null,
                };
            });

            services.AddHttpClient("GitHub", (serviceProvider, httpClient) =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<OptionsSetting>>().Value;
                string? baseUrl = config.GitHub.BaseUrl;
                httpClient.BaseAddress = new Uri(baseUrl ?? "https://hub.chrxw.com/");
                httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
                httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, nameof(XinjingdailyBot));
            });
        }
    }
}
