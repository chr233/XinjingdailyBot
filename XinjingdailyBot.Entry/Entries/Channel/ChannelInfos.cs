using SqlSugar;
using XinjingdailyBot.Entry.Columns;

namespace XinjingdailyBot.Entry.Entries.Posts;

/// <summary>
/// 来源频道设定
/// </summary>
[SugarTable("channel_info", TableDescription = "机器人频道")]
public sealed record ChannelInfos : ICreateAt, IModifyAt
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
    public string? TelegranName { get; set; }
    /// <summary>
    /// 频道名称
    /// </summary>
    [SugarColumn(IsNullable = true, Length = 512)]
    public string? Title { get; set; }

    /// <summary>
    /// 是否为公开频道
    /// </summary>
    public bool IsPublish { get; set; }

    public bool IsEnable { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;
}
