using SqlSugar;

namespace XinjingDaily.Bot.ObsoleteEntry.LegacyEntries;

/// <summary>
/// 机器人会话
/// </summary>
[Obsolete]
[SugarTable("bot_chat", TableDescription = "机器人会话")]
[SugarIndex("bot_chat_bu", nameof(BotId), OrderByType.Asc, nameof(UserId), OrderByType.Asc, true)]
[SugarIndex("bot_chat_cu", nameof(BotId), OrderByType.Asc, nameof(ChatId), OrderByType.Asc, true)]
public sealed record BotChatsLegacy
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    /// <summary>
    /// 机器人ID
    /// </summary>
    public int BotId { get; set; }
    /// <summary>
    /// 用户UID
    /// </summary>
    public long UserId { get; set; }
    /// <summary>
    /// 会话ID
    /// </summary>
    public long ChatId { get; set; }
}
