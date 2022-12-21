using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Dispatcher
{
    public interface ICommandDispatcherService
    {
        Task OnTextCommandReceived(Users dbUser, Message message);
        Task OnQueryCommandReceived(Users dbUser, CallbackQuery callbackQuery, string[] args);
    }
}
