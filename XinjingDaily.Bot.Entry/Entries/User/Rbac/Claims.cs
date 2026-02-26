using SqlSugar;

namespace XinjingDaily.Bot.Entry.Entries.Users.Rbac;


[SugarTable("claim", TableDescription = "权限表")]
public sealed record Claims
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
    public int Id { get; set; }

    /// <summary>
    /// 权限名称
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Name { get; set; }

    /// <summary>
    /// 权限Key
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Key { get; set; }
}
