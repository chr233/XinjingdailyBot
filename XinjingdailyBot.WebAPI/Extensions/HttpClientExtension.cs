using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Options;

namespace XinjingdailyBot.WebAPI.Extensions;

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
            var config = serviceProvider.GetRequiredService<IOptions<NetworkConfig>>().Value;
            string? baseUrl = config.TelegramEndpoint;
            httpClient.BaseAddress = new Uri(baseUrl ?? "https://api.telegram.org/");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(publicIdentifier, BuildInfo.Version));
        }).ConfigurePrimaryHttpMessageHandler(serviceProvider => {
            var config = serviceProvider.GetRequiredService<IOptions<NetworkConfig>>().Value;
            string? proxy = config.TelegramProxy;

            if (!string.IsNullOrEmpty(proxy))
            {
                _logger.Info("已配置代理: {0}", proxy);
                var tgProxy = new WebProxy { Address = new Uri(proxy) };

                if (proxy.StartsWith("socks"))
                {
                    return new SocketsHttpHandler { Proxy = tgProxy, UseProxy = true, };
                }
                else
                {
                    return new HttpClientHandler { Proxy = tgProxy, UseProxy = true, };
                }
            }
            else
            {
                return new HttpClientHandler();
            }
        }).RemoveAllLoggers();

        //services.AddHttpClient("GitHub", (serviceProvider, httpClient) => {
        //    var config = serviceProvider.GetRequiredService<IOptions<NetworkConfig>>().Value;
        //    string? baseUrl = config.GitHub.BaseUrl;
        //    httpClient.BaseAddress = new Uri(baseUrl ?? "https://hub.chrxw.com/");
        //    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(publicIdentifier, BuildInfo.Version));
        //});

        //services.AddHttpClient("IpInfo", (serviceProvider, httpClient) => {
        //    var config = serviceProvider.GetRequiredService<IOptions<NetworkConfig>>().Value;
        //    httpClient.BaseAddress = new Uri("https://ipinfo.io/");
        //    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(publicIdentifier, BuildInfo.Version));
        //    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.IpInfo.Token);
        //});

        services.AddHttpClient("Statistic", (serviceProvider, httpClient) => {
            var config = serviceProvider.GetRequiredService<IOptions<NetworkConfig>>().Value;
            httpClient.BaseAddress = new Uri("https://asfe.chrxw.com/");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(publicIdentifier, BuildInfo.Version));
        }).RemoveAllLoggers();
    }
}
