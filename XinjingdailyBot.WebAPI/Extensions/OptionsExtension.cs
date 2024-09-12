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

        var basePath = Path.Combine(Environment.CurrentDirectory, "config");

        config.SetBasePath(basePath);

        config.AddJsonFile("database.json", false, false);
        config.AddJsonFile("network.json", true, false);
        config.AddJsonFile("redis.json", true, false);
        config.AddJsonFile("api.json", true, false);

        config.AddEnvironmentVariables();

        builder.AddCustomOptionClass<DatabaseConfig>();
        builder.AddCustomOptionClass<NetworkConfig>();
        builder.AddCustomOptionClass<RedisConfig>();
        builder.AddCustomOptionClass<ApiConfig>();
        //builder.Services.Configure<DatabaseConfig>(builder.Configuration.GetSection(DatabaseConfig.SectionName));
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
