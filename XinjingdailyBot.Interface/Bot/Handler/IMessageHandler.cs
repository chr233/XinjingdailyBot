using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Handler
{
    /// <summary>
    /// 私聊消息处理器
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// 处理文本消息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task OnTextMessageReceived(Users dbUser, Message message);
        /// <summary>
        /// 处理非文本消息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task OnMediaMessageReceived(Users dbUser, Message message);
    }
}
