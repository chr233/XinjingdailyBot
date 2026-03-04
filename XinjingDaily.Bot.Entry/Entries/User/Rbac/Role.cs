using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Users.Rbac;

[SugarTable("role", TableDescription = "角色表")]
public sealed record Role : ICreateAt, IModifyAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 角色名
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Description { get; set; }

    public bool IsDefaultUserRole { get; set; }

    public bool IsDefaultAdminRole { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;
    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;

    // 导航：角色关联的权限列表
    [Navigate(NavigateType.OneToMany, nameof(RoleClaim.RoleId))]
    public List<RoleClaim>? RoleClaims { get; set; }

    // 导航：关联的用户-角色列表
    [Navigate(NavigateType.OneToMany, nameof(UserRole.RoleId))]
    public List<UserRole>? UserRoles { get; set; }
}
