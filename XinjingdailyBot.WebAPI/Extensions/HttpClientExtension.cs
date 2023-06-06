using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using MySqlX.XDevAPI.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using XinjingdailyBot.Infrastructure;

namespace XinjingdailyBot.WebAPI.Extensions
{
    /// <summary>
    /// HttpClient扩展
    /// </summary>
    public static class HttpClientExtension
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 注册HttpClient
        /// </summary>
        /// <param name="services"></param>
        public static void AddHttpClients(this IServiceCollection services)
        {
            string publicIdentifier = $"{nameof(XinjingdailyBot)}-{BuildInfo.Variant}";

            services.AddHttpClient("Telegram", (serviceProvider, httpClient) => {
                var config = serviceProvider.GetRequiredService<IOptions<OptionsSetting>>().Value;
                string? baseUrl = config.Bot.BaseUrl;
                httpClient.BaseAddress = new Uri(baseUrl ?? "https://api.telegram.org/");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(publicIdentifier, Utils.Version));
            }).ConfigurePrimaryHttpMessageHandler(serviceProvider => {
                var config = serviceProvider.GetRequiredService<IOptions<OptionsSetting>>().Value;
                string? proxy = config.Bot.Proxy;

                WebProxy? tgProxy = null;
                if (!string.IsNullOrEmpty(proxy))
                {
                    _logger.Info("已配置代理: {0}", proxy);
                    tgProxy = new WebProxy { Address = new Uri(proxy) };
                }
                return new HttpClientHandler {
                    Proxy = tgProxy,
                    UseProxy = tgProxy != null,
                };
            });

            services.AddHttpClient("GitHub", (serviceProvider, httpClient) => {
                var config = serviceProvider.GetRequiredService<IOptions<OptionsSetting>>().Value;
                string? baseUrl = config.GitHub.BaseUrl;
                httpClient.BaseAddress = new Uri(baseUrl ?? "https://hub.chrxw.com/");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(publicIdentifier, Utils.Version));
            });

            services.AddHttpClient("IpInfo", (serviceProvider, httpClient) => {
                var config = serviceProvider.GetRequiredService<IOptions<OptionsSetting>>().Value;
                httpClient.BaseAddress = new Uri("https://ipinfo.io/");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(publicIdentifier, Utils.Version));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.IpInfo.Token);
            });

            services.AddHttpClient("Statistic", (serviceProvider, httpClient) => {
                var config = serviceProvider.GetRequiredService<IOptions<OptionsSetting>>().Value;
                string? baseUrl = config.GitHub.BaseUrl;
                httpClient.BaseAddress = new Uri(baseUrl ?? "https://asfe.chrxw.com/");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(publicIdentifier, Utils.Version));
            });
        }
    }
}
