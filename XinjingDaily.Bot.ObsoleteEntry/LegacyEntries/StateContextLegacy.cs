using SqlSugar;

namespace XinjingDaily.Bot.Model.Legacy;

/// <summary>
/// 用户状态上下文
/// </summary>
[Obsolete]
[SugarTable("state_context", TableDescription = "用户状态上下文")]
public sealed record StateContextLegacy
{
    public int UserId { get; set; }
    public string? Context { get; set; }

    /// <summary>
    /// 用户数据
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(UserId))]
    public UsersLegacy? User { get; set; }
}
