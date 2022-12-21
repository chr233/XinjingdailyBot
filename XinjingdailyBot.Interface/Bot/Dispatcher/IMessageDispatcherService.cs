using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Dispatcher
{
    public interface IMessageDispatcherService
    {
        Task OnMessageReceived(Users dbUser, Message message);
    }
}
