using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    public interface IPostService : IBaseService<Posts>
    {
        Task AcceptPost(Posts post, Users dbUser, CallbackQuery callbackQuery);
        Task HandleMediaGroupPosts(ITelegramBotClient botClient, Users dbUser, Message message);
        Task HandleMediaPosts(ITelegramBotClient botClient, Users dbUser, Message message);
        Task HandleTextPosts(ITelegramBotClient botClient, Users dbUser, Message message);
        Task RejetPost(Posts post, Users dbUser, string rejectReason);
        Task SetPostTag(Posts post, BuildInTags tag, CallbackQuery callbackQuery);
    }
}
