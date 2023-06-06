using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Model;
using XinjingdailyBot.Interface.Helper;

namespace XinjingdailyBot.Service.Helper
{
    /// <inheritdoc cref="IHttpHelperService"/>
    [AppService(typeof(IHttpHelperService), LifeTime.Transient)]
    internal sealed class HttpHelperService : IHttpHelperService
    {
        private readonly ILogger<HttpHelperService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpHelperService(
            ILogger<HttpHelperService> logger,
            IHttpClientFactory httpClientFactory,
            IOptions<OptionsSetting> options)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            //统计
            if (options.Value.Statistic)
            {
                var client = _httpClientFactory.CreateClient("Statistic");

                StatisticTimer = new Timer(
                    async (_) => {
                        try
                        {
                            await client.GetAsync("/XinjingdailyBot");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "统计信息出错");
                        }
                    },
                    null,
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromHours(24)
                );
            }

        }

        private Timer? StatisticTimer { get; init; }

        /// <summary>
        /// 发送网络请求
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<Stream?> SendRequestToStream(string clientName, HttpRequestMessage request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(clientName);
                var httpRequestMessage = await client.SendAsync(request);
                httpRequestMessage.EnsureSuccessStatusCode();
                var contentStream = await httpRequestMessage.Content.ReadAsStreamAsync();
                return contentStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "网络请求失败");
                return null;
            }
        }

        /// <summary>
        /// 对象反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        private async Task<T?> StreamToObject<T>(Stream stream)
        {
            try
            {
                T? obj = await JsonSerializer.DeserializeAsync<T>(stream);
                return obj;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "反序列化失败");
                return default;
            }
        }

        public async Task<GitHubReleaseResponse?> GetLatestRelease()
        {
            var  request = new HttpRequestMessage(HttpMethod.Get, "/XinjingdailyBot/releases/latest");
            using var rawResponse = await SendRequestToStream("GitHub", request);
            if (rawResponse == null)
            {
                return null;
            }
            var response = await StreamToObject<GitHubReleaseResponse>(rawResponse);
            return response;
        }

        public async Task<Stream?> DownloadRelease(string? downloadUrl)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
            using var rawResponse = await SendRequestToStream("GitHub", request);
            return rawResponse;
        }

        public async Task<IpInfoResponse?> GetIpInformation(IPAddress ip)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/{ip}");
            using var rawResponse = await SendRequestToStream("IpInfo", request);
            if (rawResponse == null)
            {
                return null;
            }
            var response = await StreamToObject<IpInfoResponse>(rawResponse);
            return response;
        }

        public HttpClient CreateClient(string name) => _httpClientFactory.CreateClient(name);
    }
}
