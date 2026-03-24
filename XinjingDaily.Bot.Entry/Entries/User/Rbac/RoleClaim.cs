using SqlSugar;

namespace XinjingDaily.Bot.Entry.Entries.Users.Rbac;

[SugarTable("role_claim", TableDescription = "角色权限表")]
public sealed record RoleClaim
{
    [Obsolete("仅供 ORM 使用")]
    public RoleClaim() { }

    public RoleClaim(int roleId, int claimId)
    {
        RoleId = roleId;
        ClaimId = claimId;
        Role = null;
        Claim = null;
    }

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

    // 导航：关联角色表
    [Navigate(NavigateType.OneToOne, nameof(RoleId))]
    public Role? Role { get; set; }

    // 导航：关联权限表
    [Navigate(NavigateType.OneToOne, nameof(ClaimId))]
    public Claim? Claim { get; set; }
}
