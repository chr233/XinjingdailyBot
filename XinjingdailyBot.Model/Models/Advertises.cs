using SqlSugar;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Model.Models;

/// <summary>
/// 广告投放
/// </summary>
[SugarTable("ad", TableDescription = "广告投放")]
public sealed record Advertises : BaseModel, ICreateAt, IExpiredAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    /// <summary>
    /// 原会话ID
    /// </summary>
    public long ChatID { get; set; }
    /// <summary>
    /// 原消息ID
    /// </summary>
    public long MessageID { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    /// 是否自动置顶消息
    /// </summary>
    public bool PinMessage { get; set; }

    /// <summary>
    /// 广告发布位置
    /// </summary>
    public EAdMode Mode { get; set; } = EAdMode.None;

    /// <summary>
    /// 展示权重, 数值越大概率越高, 0为不展示
    /// </summary>
    public byte Weight { get; set; }

    /// <summary>
    /// 上次发布时间
    /// </summary>
    public DateTime LastPostAt { get; set; } = DateTime.MinValue;

    /// <summary>
    /// 广告展示次数
    /// </summary>
    public uint ShowCount { get; set; }
    /// <summary>
    /// 最大展示次数, 当次数值不为0且展示次数大于等于该值时自动禁用
    /// </summary>
    public uint MaxShowCount { get; set; }

    /// <summary>
    /// 外部链接
    /// </summary>
    [SugarColumn(Length = 1000)]
    public string? ExternalLink { get; set; }
    /// <summary>
    /// 外部链接名称
    /// </summary>
    [SugarColumn(Length = 1000)]
    public string? ExternalLinkName { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;

    /// <inheritdoc cref="IExpiredAt"/>
    [SugarColumn(OldColumnName = "ExpireAt")]
    public DateTime ExpiredAt { get; set; } = DateTime.MaxValue;
}
