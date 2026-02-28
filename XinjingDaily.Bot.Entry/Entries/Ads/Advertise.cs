using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Ads;

/// <summary>
/// 广告投放
/// </summary>
[SugarTable("advertise", TableDescription = "广告投放")]
public sealed record Advertise : ICreateAt, IExpiredAt
{
    /// <summary>
    /// 主键
    /// </summary> 
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    /// 投稿Id
    /// </summary>
    public int PostId { get; set; }

    /// <summary>
    /// 是否自动置顶消息
    /// </summary>
    public bool IsPinTop { get; set; }

    /// <summary>
    /// 是否自动置底消息
    /// </summary>
    public bool IsPinBottom { get; set; }

    /// <summary>
    /// 展示权重, 数值越大概率越高, 0为不展示
    /// </summary>
    public int Weight { get; set; }

    /// <summary>
    /// 上次发布时间
    /// </summary>
    public DateTime LastPostAt { get; set; } = DateTime.MinValue;

    public bool AutoDelete { get; set; }

    /// <summary>
    /// 外部链接1
    /// </summary>
    [SugarColumn(Length = 1000)]
    public string? Link1 { get; set; }
    /// <summary>
    /// 外部链接2
    /// </summary>
    [SugarColumn(Length = 1000)]
    public string? Link2 { get; set; }
    /// <summary>
    /// 外部链接3
    /// </summary>
    [SugarColumn(Length = 1000)]
    public string? Link3 { get; set; }
    /// <summary>
    /// 外部链接4
    /// </summary>
    [SugarColumn(Length = 1000)]
    public string? Link4 { get; set; }
    /// <summary>
    /// 外部链接5
    /// </summary>
    [SugarColumn(Length = 1000)]
    public string? Link5 { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;

    /// <inheritdoc cref="IExpiredAt"/>
    public DateTime ExpiredAt { get; set; } = DateTime.MaxValue;
}
