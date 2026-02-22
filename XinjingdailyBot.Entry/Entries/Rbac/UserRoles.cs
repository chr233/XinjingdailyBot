using SqlSugar;

namespace XinjingdailyBot.Entry.Entries.Rbac;

/// <summary>
/// 用户设置表
/// </summary>
[SugarTable("user_role", TableDescription = "用户角色表")]
public sealed record UserRoles
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
}
