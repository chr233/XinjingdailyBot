namespace XinjingDaily.Bot.Infrastructure.Configs;

/// <summary>
/// 缓存时间设置
/// </summary>
public sealed record CacheConfig
{
    public int UserPermissionTtl { get; init; } = 300;
}
