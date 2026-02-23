using SqlSugar;

namespace XinjingdailyBot.Entry.Entries.Users;

/// <summary>
/// 用户等级表
/// </summary>
[SugarTable("user_statistic", TableDescription = "用户统计信息表")]
public sealed record UserStatistics
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int Id { get; set; }
    /// <summary>
    /// UserInfo 主键
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 通过的稿件数量
    /// </summary>
    public int AcceptCount { get; set; }
    /// <summary>
    /// 被拒绝的稿件数量
    /// </summary>
    public int RejectCount { get; set; }
    /// <summary>
    /// 过期未被审核的稿件数量(统计时总投稿需要减去此字段)
    /// </summary>
    public int ExpiredPostCount { get; set; }

    /// <summary>
    /// 投稿数量
    /// </summary>
    public int PostCount { get; set; }
    /// <summary>
    /// 审核数量
    /// </summary>
    public int ReviewCount { get; set; }
    /// <summary>
    /// 经验
    /// </summary>
    public ulong Experience { get; set; }

}
