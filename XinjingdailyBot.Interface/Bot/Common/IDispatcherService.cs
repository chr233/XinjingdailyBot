using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Common
{
    public interface IDispatcherService
    {
        /// <summary>
        /// 收到CallbackQuery
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        Task OnCallbackQueryReceived(Users dbUser, CallbackQuery query);
        /// <summary>
        /// 收到频道消息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task OnChannalPostReceived(Users dbUser, Message message);
        /// <summary>
        /// 收到Query消息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        Task OnInlineQueryReceived(Users dbUser, InlineQuery query);
        /// <summary>
        /// 收到加群请求
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task OnJoinRequestReceived(Users dbUser, ChatJoinRequest request);
        /// <summary>
        /// 收到私聊或者群组消息消息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task OnMessageReceived(Users dbUser, Message message);
        /// <summary>
        /// 收到其他消息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        Task OnOtherUpdateReceived(Users dbUser, Update update);
    }
}
