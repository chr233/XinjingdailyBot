using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Channel;

/// <summary>
/// 来源频道设定
/// </summary>
[SugarTable("source_channel_setting", TableDescription = "来源频道设定")]
public sealed record SourceChannelSetting : ICreateAt, IModifyAt
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
    [SugarColumn(IsNullable = true, Length = 512)]
    public string? Title { get; set; }

    /// <summary>
    /// 是否为公开频道
    /// </summary>
    public bool IsPublish { get; set; }

    /// <summary>
    /// 是否自动拒绝频道投稿
    /// </summary>
    public bool IsAutoReject { get; set; }

    /// <summary>
    /// 允许投稿携带来源
    /// </summary>
    public bool IsKeepFrom { get; set; } = true;

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;
}
