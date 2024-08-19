using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Configs;
using XinjingdailyBot.Infrastructure.Options;

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
        var config = builder.Configuration;

        var basePath = Path.Combine(Utils.AppDir, "config");
        config.SetBasePath(basePath);

        config.AddJsonFile("log.json", true, true);
        config.AddJsonFile("database.json", false, false);
        config.AddJsonFile("system.json", false, false);

        builder.AddCustomOptionClass<BotConfig>();
    }

    /// <summary>
    /// 添加自定义配置类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    public static void AddCustomOptionClass<T>(this WebApplicationBuilder builder) where T : class, IXjbConfig
    {
        var sectionName = T.SectionName;
        if (!string.IsNullOrEmpty(sectionName))
        {
            builder.Services.Configure<T>(builder.Configuration.GetSection(sectionName));
        }
        else
        {
            builder.Services.Configure<T>(builder.Configuration);
        }
    }
}
