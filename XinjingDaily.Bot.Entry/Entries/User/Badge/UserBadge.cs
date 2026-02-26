using SqlSugar;

namespace XinjingDaily.Bot.Entry.Entries.User.Badge;

/// <summary>
/// 用户-徽章关联表
/// </summary>
[SugarTable("user_badge", TableDescription = "用户-徽章关联")]
public sealed record UserBadge
{
    /// <summary>
    /// UserInfo主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int UserId { get; set; }

    /// <summary>
    /// Badge主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int BadgeId { get; set; }

}
