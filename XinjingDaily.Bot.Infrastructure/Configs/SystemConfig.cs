using XinjingDaily.Bot.Infrastructure;

namespace XinjingDaily.Bot.Infrastructure.Options;

public sealed record SystemConfig
{
    public bool Debug { get; init; } = BuildInfo.IsDebug;
    public bool Swagger { get; init; } = BuildInfo.IsDebug;
    public int HttpPort { get; init; } = 8234;
}
