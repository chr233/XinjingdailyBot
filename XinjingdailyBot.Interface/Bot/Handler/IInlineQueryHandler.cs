using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Handler
{
    public interface IInlineQueryHandler
    {
        Task OnInlineQueryReceived(Users dbUser, InlineQuery query);
    }
}