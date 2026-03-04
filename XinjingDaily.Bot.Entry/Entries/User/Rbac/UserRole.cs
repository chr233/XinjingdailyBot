using SqlSugar;

namespace XinjingDaily.Bot.Entry.Entries.Users.Rbac;

/// <summary>
/// 用户设置表
/// </summary>
[SugarTable("user_role", TableDescription = "用户角色表")]
public sealed record UserRole
{
    /// <summary>
    /// User主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int UserId { get; set; }

    /// <summary>
    /// Role主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int RoleId { get; set; }

    // 导航：关联用户表
    [Navigate(NavigateType.OneToOne, nameof(UserId))]
    public UserInfo? User { get; set; }

    // 导航：关联角色表
    [Navigate(NavigateType.OneToOne, nameof(RoleId))]
    public Role? Role { get; set; }
}
