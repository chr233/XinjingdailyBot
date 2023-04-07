using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Handler
{
    public interface IForwardMessageHandler
    {
        Task<bool> OnForwardMessageReceived(Users dbUser, Message message);
    }
}
