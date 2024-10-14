using SqlSugar;
using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Model.Models;

/// <summary>
/// 媒体组记录
/// </summary>
[SugarTable("post_group", TableDescription = "媒体组稿件记录")]
[SugarIndex("index_msg", nameof(ChatId), OrderByType.Asc, nameof(MessageId), OrderByType.Asc)]
[SugarIndex("index_groupid", nameof(MediaGroupId), OrderByType.Asc)]
public sealed record MediaGroups : BaseModel, ICreateAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    /// <summary>
    /// 聊天ID
    /// </summary>
    public long ChatId { get; set; } = -1;
    /// <summary>
    /// 发布的消息Id
    /// </summary>
    public long MessageId { get; set; } = -1;
    /// <summary>
    /// 稿件ID
    /// </summary>
    public string MediaGroupId { get; set; } = "";
    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;
}
