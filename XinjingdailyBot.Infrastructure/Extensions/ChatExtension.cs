using Telegram.Bot.Types;

namespace XinjingdailyBot.Infrastructure.Extensions
{
    public static class ChatExtension
    {
        public static string ChatID(this Chat chat)
        {
            return string.IsNullOrEmpty(chat.Username) ? $"#{chat.Id}" : $"@{chat.Username}";
        }

        public static string ChatProfile(this Chat chat)
        {
            return $"{chat.Title} {chat.ChatID()}";
        }
    }
}
