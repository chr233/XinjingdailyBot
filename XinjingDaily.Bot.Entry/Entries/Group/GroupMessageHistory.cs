using SqlSugar;
using Telegram.Bot.Types.Enums;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Chats;

[SugarTable("group_message_history", TableDescription = "群聊消息记录")]
public sealed record GroupMessageHistory : ICreateAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
    public int Id { get; set; }

    /// <summary>
    /// ChatInfo主键
    /// </summary>
    public int ChatId { get; set; }

    /// <summary>
    /// UserInfo主键
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// MessageAttachment主键
    /// </summary>
    public int AttachmentId { get; set; } = -1;

    /// <summary>
    /// 消息ID
    /// </summary>
    public long MessageId { get; set; }

    /// <summary>
    /// 回复消息ID
    /// </summary>
    public long ReplyMessageId { get; set; } = -1;

    /// <summary>
    /// 消息内容
    /// </summary>
    [SugarColumn(Length = 2000)]
    public string Content { get; set; } = "";

    public string Type { get; set; } = nameof(MessageType.Unknown);

    public DateTime CreateAt { get; set; }
}
