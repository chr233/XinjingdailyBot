using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;
using XinjingDaily.Bot.Entry.Entries.Users.Rbac;
using XinjingDaily.Bot.Infrastructure.Extensions;

namespace XinjingDaily.Bot.Entry.Entries.Users;

/// <summary>
/// 用户基础信息表
/// </summary>
[SugarTable("user_info", TableDescription = "用户基础信息表")]
[SugarIndex("i_user_username", nameof(TelegramName), OrderByType.Asc)]
[SugarIndex("i_user_telegramid", nameof(TelegramId), OrderByType.Asc, true)]
public sealed record UserInfo : IModifyAt, ICreateAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// Telegram用户ID, 通过Telegram API获取, 全局唯一
    /// </summary>
    public long TelegramId { get; set; }

    /// <summary>
    /// 用户名@
    /// </summary>
    [SugarColumn(IsNullable = true, Length = 150)]
    public string? TelegramName { get; set; }
    /// <summary>
    /// 用户昵称 姓
    /// </summary>
    [SugarColumn(IsNullable = true, Length = 150)]
    public string? FirstName { get; set; }
    /// <summary>
    /// 用户昵称 名
    /// </summary>
    [SugarColumn(IsNullable = true, Length = 150)]
    public string? LastName { get; set; }

    /// <summary>
    /// 用户昵称
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string FullName => string.IsNullOrEmpty(LastName) ? FirstName ?? "" : $"{FirstName} {LastName}";

    /// <summary>
    /// 是否封禁
    /// </summary>
    public bool IsBan { get; set; }
    /// <summary>
    /// 是否为Bot
    /// </summary>
    public bool IsBot { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.MinValue;

    // 新增导航：用户关联的角色列表
    [Navigate(NavigateType.OneToMany, nameof(UserRole.UserId))]
    public List<UserRole>? UserRoles { get; set; }

    // 新增导航：用户直接关联的权限列表
    [Navigate(NavigateType.OneToMany, nameof(UserClaim.UserId))]
    public List<UserClaim>? UserClaims { get; set; }

    /// <summary>
    /// 文本显示
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (string.IsNullOrEmpty(TelegramName))
        {
            return $"{Id}-{FullName}(#{TelegramId})".EscapeHtml();
        }
        else
        {
            return $"{Id}-{FullName}(@{TelegramName})".EscapeHtml();
        }
    }

    /// <summary>
    /// Html链接
    /// </summary>
    /// <returns></returns>
    public string HtmlUserLink()
    {
        var nick = FullName.EscapeHtml();

        if (string.IsNullOrEmpty(TelegramName))
        {
            return $"<a href=\"tg://user?id={Id}\">{nick}</a>";
        }
        else
        {
            return $"<a href=\"https://t.me/{TelegramName}\">{nick}</a>";
        }
    }

    /// <summary>
    /// 用户名转义
    /// </summary>
    /// <returns></returns>
    public string EscapedFullName()
    {
        return FullName.EscapeHtml();
    }

    public static implicit operator int(UserInfo value) => value.Id;
}
