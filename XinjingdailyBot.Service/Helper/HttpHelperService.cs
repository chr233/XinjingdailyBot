using System.Text.Json;
using Microsoft.Extensions.Logging;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Model;

namespace XinjingdailyBot.Service.Helper
{
    [AppService(typeof(IHttpHelperService), LifeTime.Transient)]
    public sealed class HttpHelperService : IHttpHelperService
    {
        private readonly ILogger<HttpHelperService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpHelperService(
            ILogger<HttpHelperService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

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

        /// <summary>
        /// 获取最新的发行版
        /// </summary>
        /// <returns></returns>
        public async Task<GitHubReleaseResponse?> GetLatestRelease()
        {
            HttpRequestMessage request = new(HttpMethod.Get, "/XinjingdailyBot/releases/latest");
            using var rawResponse = await SendRequestToStream("GitHub", request);
            if (rawResponse == null)
            {
                return null;
            }
            var response = await StreamToObject<GitHubReleaseResponse>(rawResponse);
            return response;
        }

        /// <summary>
        /// 下载发行版
        /// </summary>
        /// <param name="downloadUrl"></param>
        /// <returns></returns>
        public async Task<Stream?> DownloadRelease(string? downloadUrl)
        {
            HttpRequestMessage request = new(HttpMethod.Get, downloadUrl);
            var rawResponse = await SendRequestToStream("GitHub", request);
            return rawResponse;
        }
    }
}
