namespace XinjingDaily.Bot.Infrastructure.Options;

public sealed record SystemConfig
{
    public bool Debug { get; init; } = BuildInfo.IsDebug;
    public bool Swagger { get; init; } = BuildInfo.IsDebug;
    public int HttpPort { get; init; } = 8235;

    /// <summary>
    /// 超级管理员 (覆盖数据库配置)
    /// </summary>
    public List<string>? SuperAdmins { get; init; }
}
