using SqlSugar;

namespace XinjingDaily.Bot.ObsoleteEntry.LegacyEntries;

/// <summary>
/// 用户等级表
/// </summary>
[Obsolete]
[SugarTable("level", TableDescription = "等级组")]
public sealed record LevelsLegacy
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int Id { get; set; }
    /// <summary>
    /// 等级名称
    /// </summary>
    public string Name { get; set; } = "";
    /// <summary>
    /// 最小经验
    /// </summary>
    public ulong MinExp { get; set; }
    /// <summary>
    /// 最高经验
    /// </summary>
    public ulong MaxExp { get; set; }
}
