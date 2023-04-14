using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Handler
{
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
