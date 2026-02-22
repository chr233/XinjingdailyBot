using SqlSugar;

namespace XinjingdailyBot.Entry.Entries.Rbac;

[SugarTable("user_claim", TableDescription = "用户角色表")]
public sealed record class UserClaims
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
