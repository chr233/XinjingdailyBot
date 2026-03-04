using SqlSugar;

namespace XinjingDaily.Bot.Entry.Entries.Users.Rbac;

[SugarTable("user_claim", TableDescription = "用户权限表")]
public sealed record class UserClaim
{
    /// <summary>
    /// User主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int UserId { get; set; }

    /// <summary>
    /// Claim主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int ClaimId { get; set; }

    // 导航：关联用户表
    [Navigate(NavigateType.OneToOne, nameof(UserId))]
    public UserInfo? User { get; set; }

    // 导航：关联权限表
    [Navigate(NavigateType.OneToOne, nameof(ClaimId))]
    public Claim? Claim { get; set; }
}
