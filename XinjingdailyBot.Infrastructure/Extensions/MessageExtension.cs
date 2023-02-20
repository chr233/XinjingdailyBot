using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace XinjingdailyBot.Infrastructure.Extensions
{
    public static class MessageExtension
    {
        public static bool CanSpoiler(this Message message)
        {
            return message.Type == MessageType.Photo || message.Type == MessageType.Video;
        }
    }
}
