using XinjingdailyBot.Infrastructure;

namespace XinjingdailyBot.WebAPI.Extensions;

/// <summary>
/// 配置文件扩展
/// </summary>
public static class OptionsExtension
{
    /// <summary>
    /// 添加自定义配置文件
    /// </summary>
    /// <param name="builder"></param>
    public static void AddCustomJsonFiles(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration
            .AddJsonFile("config.json", false, false)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>()
            .Build();

        builder.Services.Configure<OptionSettings>(config);
    }
}
