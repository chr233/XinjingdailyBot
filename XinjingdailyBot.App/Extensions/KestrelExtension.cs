using Microsoft.Extensions.Options;
using XinjingdailyBot.Infrastructure.Options;

namespace XinjingdailyBot.WebAPI.Extensions;

/// <summary>
/// WebAPI扩展
/// </summary>
public static class KestrelExtension
{
    /// <summary>
    /// 设置Kestrel
    /// </summary>
    /// <param name="webHost"></param>
    public static void SetupKestrel(this IWebHostBuilder webHost)
    {

        webHost.UseKestrel(o => {
            // 设置最大文件上传尺寸
            o.Limits.MaxRequestBodySize = 1073741824;

            var services = o.ApplicationServices;

            // 设置 Http 监听地址
            var apiOption = services.GetRequiredService<IOptions<ApiConfig>>().Value;
            var port = apiOption.Port;

            if (port < 1024)
            {
                var _logger = NLog.LogManager.GetCurrentClassLogger();
                _logger.Warn("Api.Port 不建议低于 1024, 当前设置: {port}", port);
            }

            o.ListenAnyIP(port);
        });
    }
}
