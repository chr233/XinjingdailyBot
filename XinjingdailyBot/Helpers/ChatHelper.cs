using Telegram.Bot.Types;

namespace XinjingdailyBot.Helpers
{
    internal static class ChatHelper
    {
        internal static string ChatID(this Chat chat)
        {
            return string.IsNullOrEmpty(chat.Username) ? $"#{chat.Id}" : $"@{chat.Username}";
        }

        internal static string ChatProfile(this Chat chat)
        {
            return $"{chat.Title} {chat.ChatID()}";
        }
    }
}
