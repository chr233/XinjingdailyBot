using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler
{
    public interface IInlineQueryHandler
    {
        Task OnInlineQueryReceived(Users dbUser, InlineQuery query);
    }
}