using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Dispatcher
{
    public interface IQueryDispatcherService
    {
        Task OnCallbackQueryReceived(Users dbUser, CallbackQuery callbackQuery);
    }
}