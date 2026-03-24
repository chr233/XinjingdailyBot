using SqlSugar;
using System.Text.Json.Serialization;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Users.Rbac;


[SugarTable("claim", TableDescription = "权限表")]
public sealed record Claim : ICreateAt, IModifyAt
{
    [Obsolete("仅供 ORM 使用")]
    public Claim() { }

    [JsonConstructor]
    public Claim(int id, string? name, string? value, string? description, DateTime createAt, DateTime modifyAt)
    {
        Id = id;
        Name = name;
        Value = value;
        Description = description;
        CreateAt = createAt;
        ModifyAt = modifyAt;
    }

    public Claim(int id, string name, string value, string? description = null)
    {
        Id = id;
        Name = name;
        Value = value;
        Description = description;
        CreateAt = DateTime.UtcNow;
        ModifyAt = DateTime.MinValue;
    }

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
