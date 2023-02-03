using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Localization;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using XinjingdailyBot.Infrastructure.Model;

namespace XinjingdailyBot.Service.Helper
{
    //[AppService(typeof(IMarkupHelperService), LifeTime.Transient)]
    public sealed class GithubHelperService //: IMarkupHelperService
    {
        private readonly ILogger<GithubHelperService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public GithubHelperService(
            ILogger<GithubHelperService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// 获取最新的发行版
        /// </summary>
        /// <returns></returns>
        private async Task<GitHubReleaseResponse?> GetLatestRelease()
        {
            HttpRequestMessage httpRequestMessage = new (HttpMethod.Get, "XinjingdailyBot/releases/latest");

            var httpClient = _httpClientFactory.CreateClient();
            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            httpResponseMessage.EnsureSuccessStatusCode();

            using var contentStream =
                await httpResponseMessage.Content.ReadAsStreamAsync();

            //return await JsonSerializer.DeserializeAsync
            //    <IEnumerable<GitHubBranch>>(contentStream);


            //Uri request = new(
            //    useMirror ? "https://hub.chrxw.com/ASFenhance/releases/latest" : "https://api.github.com/repos/chr233/ASFenhance/releases/latest"
            //);
            //var response = await ASF.WebBrowser!.UrlGetToJsonObject<GitHubReleaseResponse>(request).ConfigureAwait(false);

            //if (response == null && useMirror)
            //{
            //    return await GetLatestRelease(false).ConfigureAwait(false);
            //}

            //return response?.Content;
            return null;
        }

        /// <summary>
        /// 下载发行版
        /// </summary>
        /// <param name="downloadUrl"></param>
        /// <returns></returns>
        private async Task<int?> DownloadRelease(string? downloadUrl)
        {
            return null;

            //if (string.IsNullOrEmpty(downloadUrl))
            //{
            //    return null;
            //}

            //Uri request = new(downloadUrl);
            //BinaryResponse? response = await ASF.WebBrowser!.UrlGetToBinary(request).ConfigureAwait(false);
            //return response;
        }

    }
}
