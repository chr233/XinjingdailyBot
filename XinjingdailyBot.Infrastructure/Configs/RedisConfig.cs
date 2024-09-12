using XinjingdailyBot.Infrastructure.Configs;

namespace XinjingdailyBot.Infrastructure.Options;
public sealed record RedisConfig : IXjbConfig
{
    public static string SectionName => "Redis";

    public bool Enable { get; init; } = false;
    public string? Host { get; init; }
    public int Port { get; init; } = 6379;
    public string? Password { get; init; }
    public string? Prefix { get; init; }
}
