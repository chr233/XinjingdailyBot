using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Common
{
    public interface IDispatcherService
    {
        Task OnCallbackQueryReceived(Users dbUser, CallbackQuery query);
        Task OnMessageReceived(Users dbUser, Message message);
    }
}
