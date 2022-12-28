using Telegram.Bot.Types;

namespace XinjingdailyBot.Infrastructure.Extensions
{
    public static class UserExtension
    {
        public static string NickName(this User user)
        {
            return string.IsNullOrEmpty(user.LastName) ? user.FirstName : $"{user.FirstName} {user.LastName}";
        }

        public static string UserID(this User user)
        {
            return string.IsNullOrEmpty(user.Username) ? $"#{user.Id}" : $"@{user.Username}";
        }

        public static string UserProfile(this User user)
        {
            return $"{user.EscapedNickName()} {user.UserID()}";
        }

        /// <summary>
        /// HTML转义后的用户名
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string EscapedNickName(this User user)
        {
            return user.NickName().EscapeHtml();
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
}
