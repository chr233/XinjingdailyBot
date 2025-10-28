namespace XinjingdailyBot.Infrastructure.Configs;

public sealed record SystemConfig
{
    public bool Debug { get; init; } = BuildInfo.IsDebug;
    public bool Swagger { get; init; } = BuildInfo.IsDebug;

    public bool Statistic { get; init; } = true;

    public int HttpPort { get; init; } = 8234;
}
