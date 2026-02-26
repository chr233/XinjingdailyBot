using SqlSugar;

namespace XinjingDaily.Bot.Entry.Entries.Users.Rbac;

[SugarTable("role", TableDescription = "角色表")]
public sealed record Roles
{

    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 角色名
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
}
