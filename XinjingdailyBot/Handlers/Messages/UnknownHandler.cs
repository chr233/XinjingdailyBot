using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Models;

namespace XinjingdailyBot.Handlers.Messages
{
    internal sealed class UnknownHandler
    {
        public static Task ProcessMessage(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            throw new NotImplementedException();
        }

    }


}
