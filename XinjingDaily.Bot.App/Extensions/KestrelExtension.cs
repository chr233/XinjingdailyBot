using Microsoft.Extensions.Options;
using XinjingDaily.Bot.Infrastructure;

namespace XinjingDaily.Bot.WebAPI.Extensions;

/// <summary>
/// WebAPI扩展
/// </summary>
public static class KestrelExtension
{
    extension(IWebHostBuilder webHost)
    {
        /// <summary>
        /// 设置Kestrel
        /// </summary>
        /// <param name="webHost"></param>
        public void SetupKestrel()
        {

            webHost.UseKestrel(o => {
                // 设置最大文件上传尺寸
                o.Limits.MaxRequestBodySize = 30000000;

                var services = o.ApplicationServices;

                // 设置 Http 监听地址
                var apiOption = services.GetRequiredService<IOptions<AppSettings>>().Value;
                var port = apiOption.System.HttpPort;

                if (port < 1024)
                {
                    var _logger = NLog.LogManager.GetCurrentClassLogger();
                    _logger.Warn("System.Port 不建议低于 1024, 当前设置: {port}", port);
                }

                o.ListenAnyIP(port);
            });
        }
    }
}
