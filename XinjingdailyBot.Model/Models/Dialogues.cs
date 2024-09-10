using SqlSugar;
using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Model.Columns;
using XinjingdailyBot.Model.Services;

namespace XinjingdailyBot.Model.Models;

/// <summary>
/// 消息记录
/// </summary>
[SplitTable(SplitType._Custom01, typeof(ChatIdSplitService))]
[SugarTable("dialogue", TableDescription = "消息记录")]
[SugarIndex("index_chat", nameof(ChatID), OrderByType.Asc, nameof(MessageID), OrderByType.Asc, true)]
public sealed record Dialogues : BaseModel, ICreateAt
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
    public long ChatID { get; set; }
    /// <summary>
    /// 消息ID
    /// </summary>
    public long MessageID { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserID { get; set; } = -1;

    /// <summary>
    /// 回复消息ID
    /// </summary>
    public long ReplyMessageID { get; set; } = -1;

    /// <summary>
    /// 消息内容
    /// </summary>
    [SugarColumn(Length = 2000)]
    public string Content { get; set; } = "";

    /// <summary>
    /// 消息类型
    /// </summary>
    public string Type { get; set; } = "";

    /// <inheritdoc cref="ICreateAt"/>
    [SugarColumn(OldColumnName = "Data", DefaultValue = "1970-01-01 00:00:00")]
    public DateTime CreateAt { get; set; }
}
