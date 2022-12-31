using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Handler
{
    public interface IGroupMessageHandler
    {
        Task OnGroupTextMessageReceived(Users dbUser, Message message);
    }
}