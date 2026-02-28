using XinjingDaily.Bot.Infrastructure.Configs;
using XinjingDaily.Bot.Infrastructure.Options;

namespace XinjingDaily.Bot.Infrastructure;

/// <summary>
/// 机器人配置
/// </summary>
public sealed record AppSettings
{
    /// <inheritdoc cref="SystemConfig"/>
    public SystemConfig System { get; init; } = new();

    /// <inheritdoc cref="BotConfig"/>
    public BotConfig Bot { get; init; } = new();

    /// <inheritdoc cref="DatabaseConfig"/>
    public DatabaseConfig Database { get; init; } = new();

    /// <inheritdoc cref="RedisConfig"/>
    public RedisConfig Redis { get; init; } = new();

    /// <inheritdoc cref="NetworkConfig"/>
    public NetworkConfig Network { get; init; } = new();

    /// <inheritdoc cref="IpInfoConfig"/>
    public IpInfoConfig IpInfo { get; init; } = new();
}
