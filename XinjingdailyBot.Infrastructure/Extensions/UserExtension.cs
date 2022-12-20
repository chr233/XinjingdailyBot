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
            return $"{user.EscapedUserName()} {user.UserID()}";
        }

        /// <summary>
        /// HTML转义
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string EscapeHtml(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }
            else
            {
                var escapedText = text
                    .Replace("<", "＜")
                    .Replace(">", "＞")
                    .Replace("&", "＆");
                return escapedText;
            }
        }

        /// <summary>
        /// HTML转义后的聊天名
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string EscapedChatName(this Chat chat)
        {
            return EscapeHtml(chat.Title);
        }

        /// <summary>
        /// HTML转义后的用户名
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string EscapedUserName(this User user)
        {
            return EscapeHtml(user.NickName());
        }

    }
}
