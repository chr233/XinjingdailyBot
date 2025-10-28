namespace XinjingdailyBot.Infrastructure.Configs;

/// <summary>
/// 消息选项
/// </summary>
public sealed record MessageOption
{
    /// <summary>
    /// /start 命令返回的消息
    /// </summary>
    public string? Start { get; set; }
    /// <summary>
    /// /help 命令返回的消息
    /// </summary>
    public string? Help { get; set; }
    /// <summary>
    /// /about 命令返回的消息
    /// </summary>
    public string? About { get; set; }
}
