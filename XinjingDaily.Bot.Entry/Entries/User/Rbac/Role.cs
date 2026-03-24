using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Users.Rbac;

[SugarTable("role", TableDescription = "角色表")]
public sealed record Role : ICreateAt, IModifyAt
{
    [Obsolete("仅供 ORM 使用")]
    public Role () { }
   
    public Role(int id, string? name, string? description, bool isDefaultUserRole, DateTime createAt, DateTime modifyAt, List<RoleClaim>? roleClaims, List<UserRole>? userRoles)
    {
        Id = id;
        Name = name;
        Description = description;
        IsDefaultUserRole = isDefaultUserRole;
        CreateAt = createAt;
        ModifyAt = modifyAt;
        RoleClaims = roleClaims;
        UserRoles = userRoles;
    }

    public Role(int id, string? name, string? description, bool isDefaultUserRole)
    {
        Id = id;
        Name = name;
        Description = description;
        IsDefaultUserRole = isDefaultUserRole;
        CreateAt = DateTime.UtcNow;
        ModifyAt = DateTime.MinValue;
        RoleClaims = null;
        UserRoles = null;
    }

    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
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

    /// <summary>
    /// 新用户默认权限组
    /// </summary>
    public bool IsDefaultUserRole { get; set; }


    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.MinValue;

    // 导航：角色关联的权限列表
    [Navigate(NavigateType.OneToMany, nameof(RoleClaim.RoleId))]
    public List<RoleClaim>? RoleClaims { get; set; }

    // 导航：关联的用户-角色列表
    [Navigate(NavigateType.OneToMany, nameof(UserRole.RoleId))]
    public List<UserRole>? UserRoles { get; set; }
}
