using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Handler
{
    public interface IMessageHandler
    {
        Task OnTextMessageReceived(Users dbUser, Message message);
        Task OnMediaMessageReceived(Users dbUser, Message message);
    }
}
