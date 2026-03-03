using SqlSugar;
using Telegram.Bot.Types.Enums;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Chat;

/// <summary>
/// 发布频道表
/// </summary>
[SugarTable("chat_info", TableDescription = "群聊/频道信息")]
[SugarIndex("i_chat_info_telegram_id", nameof(TelegramId), OrderByType.Asc, true)]
[SugarIndex("i_chat_info_telegram_name", nameof(TelegramName), OrderByType.Asc, true)]
public sealed record ChatInfo : ICreateAt, IModifyAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    /// <summary>
    /// 频道ID
    /// </summary>
    public long TelegramId { get; set; }
    /// <summary>
    /// 频道ID @
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? TelegramName { get; set; }
    /// <summary>
    /// 频道名称
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Title { get; set; }

    /// <summary>
    /// 会话类型
    /// </summary>
    public ChatType Type { get; set; }

    /// <summary>
    /// 是否为群组
    /// </summary>
    public bool IsGroup => Type is ChatType.Group or ChatType.Supergroup;

    /// <summary>
    /// 是否为频道
    /// </summary>
    public bool IsChannel => Type is ChatType.Channel;

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;
}
