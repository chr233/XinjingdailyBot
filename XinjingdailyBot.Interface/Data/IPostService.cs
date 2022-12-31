using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    public interface IPostService : IBaseService<Posts>
    {
        int MaxPostText { get; }

        Task AcceptPost(Posts post, Users dbUser, CallbackQuery callbackQuery);
        Task HandleMediaGroupPosts(Users dbUser, Message message);
        Task HandleMediaPosts(Users dbUser, Message message);
        Task HandleTextPosts(Users dbUser, Message message);
        Task RejetPost(Posts post, Users dbUser, string rejectReason);
        Task SetPostTag(Posts post, BuildInTags tag, CallbackQuery callbackQuery);
    }
}
