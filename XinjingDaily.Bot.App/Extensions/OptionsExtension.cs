using XinjingDaily.Bot.Infrastructure;

namespace XinjingDaily.Bot.WebAPI.Extensions;

/// <summary>
/// 配置文件扩展
/// </summary>
public static class OptionsExtension
{
    extension(WebApplicationBuilder builder)
    {
        /// <summary>
        /// 添加自定义配置文件
        /// </summary>
        /// <param name="builder"></param>
        public void AddCustomJsonFiles()
        {
            var config = builder.Configuration
                .AddJsonFile("config.json", false, false)
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>()
                .Build();

            builder.Services.Configure<AppSettings>(config);
        }
    }
}
