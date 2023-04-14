using System.Net;
using XinjingdailyBot.Infrastructure.Model;

namespace XinjingdailyBot.Interface.Helper
{
    public interface IHttpHelperService
    {
        /// <summary>
        /// 下载发行版
        /// </summary>
        /// <param name="downloadUrl"></param>
        /// <returns></returns>
        Task<Stream?> DownloadRelease(string? downloadUrl);
        /// <summary>
        /// 获取IP信息
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        Task<IpInfoResponse?> GetIpInformation(IPAddress ip);
        /// <summary>
        /// 获取最新的发行版
        /// </summary>
        /// <returns></returns>
        Task<GitHubReleaseResponse?> GetLatestRelease();
    }
}
