using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler
{
    public interface IGroupMessageHandler
    {
        Task OnGroupTextMessageReceived(Users dbUser, Message message);
    }
}