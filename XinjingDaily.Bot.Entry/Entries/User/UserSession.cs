using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Users;

/// <summary>
/// 用户会话表
/// </summary>
[SugarTable("user_session", TableDescription = "用户会话")]
[SugarIndex("i_user_session_user_id", nameof(UserId), OrderByType.Asc, true)]
public sealed record UserSession : ICreateAt, IModifyAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int Id { get; set; }
    /// <summary>
    /// UserInfo 主键
    /// </summary>
    public int UserId { get; set; }


    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.MinValue;
}
