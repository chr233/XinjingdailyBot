using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Handler
{
    /// <summary>
    /// 群组消息处理器
    /// </summary>
    public interface IGroupMessageHandler
    {
        /// <summary>
        /// 处理群组消息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task OnGroupTextMessageReceived(Users dbUser, Message message);
    }
}
