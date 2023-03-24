using System.Net;
using XinjingdailyBot.Infrastructure.Model;

namespace XinjingdailyBot.Interface.Helper
{
    public interface IHttpHelperService
    {
        Task<Stream?> DownloadRelease(string? downloadUrl);
        Task<IpInfoResponse?> GetIpInformation(IPAddress ip);
        Task<GitHubReleaseResponse?> GetLatestRelease();
    }
}
