using SqlSugar;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models;

/// <summary>
/// 用户状态上下文
/// </summary>
[SugarTable("state_context", TableDescription = "用户状态上下文")]
public sealed record StateContext : BaseModel
{
    public int UserId { get; set; }
    public string? Context { get; set; }

    /// <summary>
    /// 用户数据
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(UserId))]
    public Users? User { get; set; }
}
