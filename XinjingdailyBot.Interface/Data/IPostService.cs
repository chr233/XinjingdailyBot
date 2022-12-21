using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    public interface IPostService : IBaseService<Posts>
    {
        Task HandleMediaGroupPosts(ITelegramBotClient botClient, Users dbUser, Message message);
        Task HandleMediaPosts(ITelegramBotClient botClient, Users dbUser, Message message);
        Task HandleTextPosts(ITelegramBotClient botClient, Users dbUser, Message message);
    }
}
