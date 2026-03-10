using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Users.Rbac;


[SugarTable("claim", TableDescription = "权限表")]
public sealed record Claim : ICreateAt, IModifyAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsIdentity = false, IsPrimaryKey = true)]
    public int Id { get; set; }

    /// <summary>
    /// 权限名称
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Name { get; set; }

    /// <summary>
    /// 权限字段
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Value { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Description { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.MinValue;
}
