using SqlSugar;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Model.Legacy;

/// <summary>
/// 消息记录
/// </summary>
[Obsolete]
[SugarTable(TableName = "dialogue_history", TableDescription = "消息记录")]
[SugarIndex("index_chat", nameof(ChatId), OrderByType.Asc, nameof(MessageId), OrderByType.Asc, true)]
[SugarIndex("index_chat", nameof(MessageId), OrderByType.Asc, true)]
public sealed record DialogueHistorysLegacy : ICreateAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public long Id { get; set; }
    /// <summary>
    /// 会话ID
    /// </summary>
    [SplitField]
    public long ChatId { get; set; }
    /// <summary>
    /// 消息ID
    /// </summary>
    public long MessageId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; } = -1;

    /// <summary>
    /// 回复消息ID
    /// </summary>
    public long ReplyMessageId { get; set; } = -1;

    /// <summary>
    /// 消息内容
    /// </summary>
    [SugarColumn(Length = 2000)]
    public string Content { get; set; } = "";

    /// <summary>
    /// 消息类型
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// 用户数据
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(UserId))]
    public UsersLegacy? User { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; }
}
