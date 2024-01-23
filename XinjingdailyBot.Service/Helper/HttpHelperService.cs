using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Model;
using XinjingdailyBot.Interface.Helper;

namespace XinjingdailyBot.Service.Helper;

/// <inheritdoc cref="IHttpHelperService"/>
[AppService(typeof(IHttpHelperService), LifeTime.Transient)]
public sealed class HttpHelperService(
    ILogger<HttpHelperService> _logger,
    IHttpClientFactory _httpClientFactory,
    IOptions<OptionsSetting> _options) : IHttpHelperService
{

    /// <inheritdoc/>
    public HttpClient CreateClient(string name) => _httpClientFactory.CreateClient(name);

    /// <inheritdoc/>
    public Task SendStatistic()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Statistic");
            return client.GetAsync("/XinjingdailyBot");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "统计信息出错");
            return Task.CompletedTask;
        }
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
            var obj = await JsonSerializer.DeserializeAsync<T>(stream);
            return obj;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "反序列化失败");
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task<GitHubReleaseResponse?> GetLatestRelease()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/XinjingdailyBot/releases/latest");
        using var rawResponse = await SendRequestToStream("GitHub", request);
        if (rawResponse == null)
        {
            return null;
        }
        var response = await StreamToObject<GitHubReleaseResponse>(rawResponse);
        return response;
    }

    /// <inheritdoc/>
    public async Task<Stream?> DownloadRelease(string? downloadUrl)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
        var rawResponse = await SendRequestToStream("GitHub", request);
        return rawResponse;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<Stream?> GetTelegramFileHeader(Telegram.Bot.Types.File tgFile, int length)
    {
        var token = _options.Value.Bot.BotToken;
        var filePath = tgFile.FilePath;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/file/bot{token}/{filePath}");
        request.Headers.Add("Range", $"bytes=0-{length}");
        var rawStream = await SendRequestToStream("Telegram", request);
        return rawStream;
    }
}
