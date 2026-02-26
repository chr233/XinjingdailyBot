using SqlSugar;

namespace XinjingDaily.Bot.Entry.Entries.Users.Rbac;

[SugarTable("role_claim", TableDescription = "角色权限表")]
public sealed record RoleClaims
{
    /// <summary>
    /// Role主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int RoleId { get; set; }

    /// <summary>
    /// Claim主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int ClaimId { get; set; }
}
