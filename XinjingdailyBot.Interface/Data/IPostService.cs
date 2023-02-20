using Telegram.Bot.Types;
using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    public interface IPostService : IBaseService<Posts>
    {
        /// <summary>
        /// 文字投稿长度上限
        /// </summary>
        public static int MaxPostText { get; } = 2000;

        Task AcceptPost(Posts post, Users dbUser, CallbackQuery callbackQuery);
        Task<bool> CheckPostLimit(Users dbUser, Message? message = null, CallbackQuery? query = null);
        Task HandleMediaGroupPosts(Users dbUser, Message message);
        Task HandleMediaPosts(Users dbUser, Message message);
        Task HandleTextPosts(Users dbUser, Message message);
        Task RejetPost(Posts post, Users dbUser, string rejectReason);
        Task SetPostTag(Posts post, int tagId, CallbackQuery callbackQuery);
        Task SetPostTag(Posts post, string payload, CallbackQuery callbackQuery);
    }
}
