using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Infrastructure.Utils;

namespace XinjingDaily.Bot.App.Extensions;

/// <summary>
/// HttpClient扩展
/// </summary>
public static class HttpClientExtension
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private const DecompressionMethods DefaultDecompressMethod = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli;

    /// <summary>
    /// 创建HttpClientHandler
    /// </summary>
    /// <param name="name"></param>
    /// <param name="proxyAddress"></param>
    /// <returns></returns>
    internal static HttpMessageHandler CreateHttpClientHandler(string name, string? proxyAddress)
    {
        var proxy = CreateWebProxy(proxyAddress);
        if (proxy != null && !string.IsNullOrEmpty(proxyAddress))
        {
            if (proxyAddress.StartsWith("socks", StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.Info("{0} 已配置 Http 代理: {1}", name, proxyAddress);
                return new SocketsHttpHandler {
                    AutomaticDecompression = DefaultDecompressMethod,
                    Proxy = proxy,
                    UseProxy = true
                };
            }
            else if (proxyAddress.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.Info("{0} 已配置 Socks 代理: {1}", name, proxyAddress);
                return new HttpClientHandler {
                    AutomaticDecompression = DefaultDecompressMethod,
                    Proxy = proxy,
                    UseProxy = true
                };
            }
            else
            {
                _logger.Warn("{0} 无效的代理: {1}", name, proxyAddress);
            }
        }

        return new HttpClientHandler {
            AutomaticDecompression = DefaultDecompressMethod,
        };
    }

    /// <summary>
    /// 创建WebProxy
    /// </summary>
    /// <param name="proxy"></param>
    /// <returns></returns>
    private static WebProxy? CreateWebProxy(string? proxy)
    {
        if (string.IsNullOrEmpty(proxy))
        {
            return null;
        }

        if (!Uri.TryCreate(proxy, UriKind.Absolute, out var webProxy))
        {
            _logger.Warn("代理地址无效: {proxy}", proxy);
            return null;
        }

        return new WebProxy(webProxy) {
            Address = webProxy,
        };
    }

    /// <summary>
    /// 创建HttpClient
    /// </summary>
    /// <param name="cookieContainer"></param>
    /// <param name="proxy"></param>
    /// <returns></returns>
    internal static HttpMessageHandler CreateHttpClientHandler(CookieContainer cookieContainer, string? proxy)
    {
        var webProxy = CreateWebProxy(proxy);
        return new HttpClientHandler {
            CookieContainer = cookieContainer,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
            UseProxy = webProxy != null,
            Proxy = webProxy
        };
    }

    internal static Uri? GetBaseAddress(string name, string baseUrl)
    {
        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
        {
            return uriResult;
        }

        _logger.Error("{0} 配置的 BaseUrl 无效: {1}", name, baseUrl);
        _logger.Error("网络配置有误, 请检查 Network 节配置");
        SystemUtils.Shutdown();
        return null;
    }

    private const string GitHubClient = "github_client";
    private const string IpInfoClient = "ipinfo_client";
    private const string StatisticClient = "statistic_client";

    extension(IServiceCollection services)
    {
        /// <summary>
        /// 注册HttpClient
        /// </summary>
        /// <param name="services"></param>
        public void AddHttpClients()
        {
            string publicIdentifier = $"{BuildInfo.AppName}-{BuildInfo.Variant}";

            services.AddHttpClient(GitHubClient, (serviceProvider, httpClient) => {
                var config = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value.Network;
                httpClient.BaseAddress = GetBaseAddress(GitHubClient, config.GitHubApi);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(publicIdentifier, BuildInfo.Version));
            }).ConfigurePrimaryHttpMessageHandler(serviceProvider => {
                var config = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value.Network;
                string? proxy = config.WebProxy;
                return CreateHttpClientHandler(GitHubClient, proxy);
            }).RemoveAllLoggers();

            services.AddHttpClient(IpInfoClient, (serviceProvider, httpClient) => {
                var config = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value.Network;
                httpClient.BaseAddress = GetBaseAddress(IpInfoClient, config.IpInfoApi);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(publicIdentifier, BuildInfo.Version));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.IpInfoToken);
            }).ConfigurePrimaryHttpMessageHandler(serviceProvider => {
                var config = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value.Network;
                string? proxy = config.WebProxy;
                return CreateHttpClientHandler(IpInfoClient, proxy);
            }).RemoveAllLoggers();

            services.AddHttpClient(StatisticClient, (serviceProvider, httpClient) => {
                httpClient.BaseAddress = new Uri("https://asfe.chrxw.com/");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(publicIdentifier, BuildInfo.Version));
            }).ConfigurePrimaryHttpMessageHandler(serviceProvider => {
                var config = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value.Network;
                string? proxy = config.WebProxy;
                return CreateHttpClientHandler(StatisticClient, proxy);
            }).RemoveAllLoggers();
        }
    }
}
