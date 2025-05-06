using SqlSugar;
using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Model.Legacy;

/// <summary>
/// 用户曾用名记录
/// </summary>
[Obsolete]
[SugarTable("name_history", TableDescription = "用户名历史记录")]
public sealed record NameHistoryLegacy : BaseModel, ICreateAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    /// <summary>
    /// 用户UID
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// 用户昵称 姓
    /// </summary>
    public string FirstName { get; set; } = "";
    /// <summary>
    /// 用户昵称 名
    /// </summary>
    public string LastName { get; set; } = "";
    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; }
}
