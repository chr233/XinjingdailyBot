using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Handler;

/// <summary>
/// 转发消息处理器
/// </summary>
public interface IForwardMessageHandler
{
    /// <summary>
    /// 处理转发的消息
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<bool> OnForwardMessageReceived(Users dbUser, Message message);
}
