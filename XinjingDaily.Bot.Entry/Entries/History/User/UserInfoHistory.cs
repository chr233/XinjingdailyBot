using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.History.User;

/// <summary>
/// 用户信息变更记录
/// </summary>
[SugarTable("user_info_history", TableDescription = "用户信息变更记录")]
[SugarIndex("index_user_id", nameof(UserId), OrderByType.Asc)]
public sealed record UserInfoHistory : ICreateAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    /// <summary>
    /// UserInfo主键
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 姓
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// 名
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// 是否修改昵称
    /// </summary>
    public bool IsNickChanged { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? TelegramName { get; set; }

    /// <summary>
    /// 是否修改用户名
    /// </summary>
    public bool IsTelegramNameChanged { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; }
}
