using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Channel;

/// <summary>
/// 来源频道设定
/// </summary>
[SugarTable("bot_channel", TableDescription = "投稿发布频道")]
public sealed record BotChannels : ICreateAt, IModifyAt
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

    public bool IsEnable { get; set; }

    public string? ShortName { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;
}
