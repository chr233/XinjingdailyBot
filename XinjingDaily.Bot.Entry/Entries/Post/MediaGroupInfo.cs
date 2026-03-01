using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Posts;

/// <summary>
/// 多图投稿记录表
/// </summary>
[SugarTable("media_group_info", TableDescription = "多图投稿记录")]
public sealed record MediaGroupInfo : ICreateAt
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
