using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Context;

/// <summary>
/// 用户上下文表（每个用户在每个会话中独立一份）
/// </summary>
[SugarTable("user_context", TableDescription = "用户上下文")]
[SugarIndex("i_user_context_uid_cid", nameof(UserId), OrderByType.Asc, nameof(ChatId), OrderByType.Asc, true)]
public sealed record UserContextEntry : IModifyAt
{
    /// <summary>主键</summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>UserInfo 主键</summary>
    public int UserId { get; set; }

    /// <summary>Telegram Chat ID（私聊为用户 TelegramId，群聊为群 ID）</summary>
    public long ChatId { get; set; }

    /// <summary>当前会话 Mode</summary>
    [SugarColumn(IsNullable = true, Length = 100)]
    public string? Mode { get; set; }

    /// <summary>KV 数据 JSON，上限 2000 字符</summary>
    [SugarColumn(Length = 2000)]
    public string DataJson { get; set; } = "{}";

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.MinValue;
}
