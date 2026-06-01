namespace XinjingDaily.Bot.Infrastructure.Configs;

/// <summary>
/// Context 系统配置
/// </summary>
public sealed record ContextConfig
{
    /// <summary>
    /// Redis TTL（秒），默认 1800（30 分钟）。
    /// TTL 内数据缓存于 Redis；Save 时双写 Redis + DB。
    /// </summary>
    public int TtlSeconds { get; init; } = 1800;
}
