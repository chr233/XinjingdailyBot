using Telegram.Bot.Types;

namespace XinjingdailyBot.Infrastructure.Extensions;

/// <summary>
/// User扩展
/// </summary>
public static class UserExtension
{
    /// <summary>
    /// 获取用户全名
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string FullName(this User user)
    {
        return string.IsNullOrEmpty(user.LastName) ? user.FirstName : $"{user.FirstName} {user.LastName}";
    }

    /// <summary>
    /// 获取用户ID
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string UserID(this User user)
    {
        return string.IsNullOrEmpty(user.Username) ? $"#{user.Id}" : $"@{user.Username}";
    }

    /// <summary>
    /// 获取用户摘要信息
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string UserProfile(this User user)
    {
        return $"{user.EscapedNickName()} {user.UserID()}";
    }

    /// <summary>
    /// 打印用户信息
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string UserToString(this User user)
    {
        if (string.IsNullOrEmpty(user.Username))
        {
            return $"{user.FullName()}(#{user.Id})";
        }
        else
        {
            return $"{user.FullName()}(@{user.Username})";
        }
    }

    /// <summary>
    /// HTML转义后的用户名
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string EscapedNickName(this User user)
    {
        return user.FullName().EscapeHtml();
    }

    /// <summary>
    /// Html格式的用户链接
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string HtmlUserLink(this User user)
    {
        string nick = user.EscapedNickName();

        if (string.IsNullOrEmpty(user.Username))
        {
            return $"<a href=\"tg://user?id={user.UserID}\">{nick}</a>";
        }
        else
        {
            return $"<a href=\"https://t.me/{user.Username}\">{nick}</a>";
        }
    }
}
