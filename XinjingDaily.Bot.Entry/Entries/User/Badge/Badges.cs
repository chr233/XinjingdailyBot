using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.User.Badge;

/// <summary>
/// 徽章信息表
/// </summary>
[SugarTable("badge", TableDescription = "徽章信息")]
public sealed record Badges : ICreateAt, IModifyAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(IsNullable =true)]
    public string? Name { get; set; }

    [SugarColumn(IsNullable =true)]
    public string? Description { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;
}