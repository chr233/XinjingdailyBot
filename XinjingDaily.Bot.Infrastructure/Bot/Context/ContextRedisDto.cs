namespace XinjingDaily.Bot.Infrastructure.Bot.Context;

/// <summary>
/// Context 在 Redis 中的序列化结构。
/// 同时用于 UserContext 和 ChatContext。
/// </summary>
public sealed class ContextRedisDto
{
    /// <summary>DB 主键，0 表示尚未写入 DB</summary>
    public int DbId { get; set; }

    /// <summary>UserInfo 主键（UserContext 专用）</summary>
    public int UserId { get; set; }

    /// <summary>Telegram Chat ID</summary>
    public long ChatId { get; set; }

    /// <summary>命令名（ChatContext 专用）</summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>当前会话 Mode</summary>
    public string Mode { get; set; } = string.Empty;

    /// <summary>KV 数据（key → JSON 字符串）</summary>
    public Dictionary<string, string> Data { get; set; } = [];

    /// <summary>最后修改时间（UTC）</summary>
    public DateTime ModifyAt { get; set; } = DateTime.UtcNow;
}
