namespace XinjingDaily.Bot.Infrastructure.Configs;

/// <summary>
/// 缓存时间设置
/// </summary>
public sealed record CacheConfig
{
    public int UserPermissionTtl { get; init; } = 300;

    /// <summary>
    /// Redis TTL（秒），默认 1800（30 分钟）。
    /// TTL 内数据仅存 Redis；TTL 到期前每次 Save 均双写 Redis + DB。
    /// </summary>
    public int ContextTtl { get; init; } = 1800;
}
