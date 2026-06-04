using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Context;

/// <summary>
/// 群聊公共上下文表（每个命令在每个会话中独立一份，所有人共享）
/// </summary>
[SugarTable("chat_context", TableDescription = "群聊公共上下文")]
[SugarIndex("i_chat_context_cmd_cid", nameof(Command), OrderByType.Asc, nameof(ChatId), OrderByType.Asc, true)]
public sealed record ChatContextEntry : IModifyAt
{
    /// <summary>主键</summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>命令名（大写）</summary>
    [SugarColumn(Length = 100)]
    public string Command { get; set; } = "";

    /// <summary>Telegram Chat ID</summary>
    public long ChatId { get; set; }

    /// <summary>当前公共 Mode</summary>
    [SugarColumn(IsNullable = true, Length = 100)]
    public string? Mode { get; set; }

    /// <summary>KV 数据 JSON，上限 2000 字符</summary>
    [SugarColumn(Length = 2000)]
    public string DataJson { get; set; } = "{}";

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.MinValue;
}