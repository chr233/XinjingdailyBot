using Microsoft.Extensions.Options;
using StackExchange.Redis;
using XinjingdailyBot.Infrastructure.Options;

namespace XinjingdailyBot.WebAPI.Extensions;

/// <summary>
/// HttpClient扩展
/// </summary>
public static class RedisExtension
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 注册HttpClient
    /// </summary>
    /// <param name="services"></param>
    public static void AddRedis(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp => {
            var config = sp.GetRequiredService<IOptions<RedisConfig>>().Value;

            var connStr = $"{config.Host}:{config.Port}";

            var option = ConfigurationOptions.Parse(connStr);

            if (!string.IsNullOrEmpty(config.Prefix))
            {
                option.ChannelPrefix = RedisChannel.Literal(config.Prefix);
            }
            option.Password = config.Password;

            return ConnectionMultiplexer.Connect(option);
        });
    }
}
