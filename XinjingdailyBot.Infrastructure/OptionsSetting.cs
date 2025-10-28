using XinjingdailyBot.Infrastructure.Configs;

namespace XinjingdailyBot.Infrastructure;

/// <summary>
/// 机器人配置
/// </summary>
public sealed record OptionsSetting
{
    /// <inheritdoc cref="SystemConfig"/>
    public SystemConfig System { get; set; } = new();

    /// <inheritdoc cref="BotConfig"/>
    public BotConfig Bot { get; set; } = new();

    /// <inheritdoc cref="ChannelOption"/>
    public ChannelOption Channel { get; set; } = new();

    /// <inheritdoc cref="MessageOption"/>
    public MessageOption Message { get; set; } = new();

    /// <inheritdoc cref="DatabaseConfig"/>
    public DatabaseConfig Database { get; set; } = new();

    /// <inheritdoc cref="PostOption"/>
    public PostOption Post { get; set; } = new();

    /// <inheritdoc cref="GitHubOption"/>
    public GitHubOption GitHub { get; set; } = new();

    /// <inheritdoc cref="IpInfoOption"/>
    public IpInfoOption IpInfo { get; set; } = new();

    /// <inheritdoc cref="ScheduleOption"/>
    public ScheduleOption Schedule { get; set; } = new();

    /// <inheritdoc cref="LevelOption"/>
    public LevelOption Level { get; set; } = new();
}
