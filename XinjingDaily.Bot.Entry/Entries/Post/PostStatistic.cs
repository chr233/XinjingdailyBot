using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Posts;

/// <summary>
/// 投稿统计信息表
/// </summary>
[SugarTable("post_statistic", TableDescription = "投稿统计信息")]
public sealed record PostStatistic : IModifyAt, ICreateAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// PostInfo主键
    /// </summary>
    public int PostId { get; set; }

    public int ViewCount { get; set; }

    public int ReactionCount { get; set; }

    public int PostiveReactionRate { get; set; }

    public int ReplyCount { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;
    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;
}
