using SqlSugar;

namespace XinjingDaily.Bot.Entry.Entries.Posts;

/// <summary>
/// 发布频道关联标签表
/// </summary>
[SugarTable("post_channel_tag", TableDescription = "发布频道关联标签")]
public sealed record PostChannelTag
{
    /// <summary>
    /// PostChannelSetting主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int ChannelId { get; set; }

    /// <summary>
    /// PostTag主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int TagId { get; set; }
}
