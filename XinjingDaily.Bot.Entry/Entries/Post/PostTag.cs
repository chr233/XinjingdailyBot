using SqlSugar;

namespace XinjingDaily.Bot.Entry.Entries.Posts;

/// <summary>
/// 投稿标签表
/// </summary>
[SugarTable("post_tag", TableDescription = "投稿标签关联表")]
public sealed record PostTag
{
    /// <summary>
    /// PostInfo主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int PostId { get; set; }

    /// <summary>
    /// TagInfo主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int TagId { get; set; }

}
