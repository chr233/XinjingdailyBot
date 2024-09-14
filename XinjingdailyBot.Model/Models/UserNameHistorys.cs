using SqlSugar;
using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Model.Models;

/// <summary>
/// 用户曾用名记录
/// </summary>
[SugarTable("user_name_history", TableDescription = "用户名历史记录")]
public sealed record UserNameHistorys : BaseModel, ICreateAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    /// <summary>
    /// 用户UID
    /// </summary>
    public long UserId { get; set; }
    /// <summary>
    /// 用户名 @
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Username { get; set; }
    /// <summary>
    /// 用户昵称 姓
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? FirstName { get; set; }
    /// <summary>
    /// 用户昵称 名
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? LastName { get; set; }
    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; }
}
