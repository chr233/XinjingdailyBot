using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Ads;

/// <summary>
/// 广告投放
/// </summary>
[SugarTable("advertise_statistic", TableDescription = "广告统计")]
[SugarIndex("i_advertise_statistic_ad_id", nameof(AdId), OrderByType.Asc, true)]
public sealed record AdvertiseStatistic : ICreateAt, IModifyAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// Advertise主键
    /// </summary>
    public int AdId { get; set; }

    /// <summary>
    /// 广告展示次数
    /// </summary>
    public int PostCount { get; set; }

    /// <summary>
    /// 回应次数
    /// </summary>
    public int ReactionCount { get; set; }

    public bool IsDeleted { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.MinValue;
}
