using SqlSugar;

namespace XinjingdailyBot.Entry.Entries.System;

[SugarTable("role", TableDescription = "系统角色表")]
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
