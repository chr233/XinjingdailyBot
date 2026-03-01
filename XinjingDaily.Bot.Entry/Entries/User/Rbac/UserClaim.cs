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
}
