using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Handler;

/// <summary>
/// 频道消息处理器
/// </summary>
public interface IChannelPostHandler
{
    /// <summary>
    /// 自动更新频道发布的多媒体消息
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task OnMediaChannelPostReceived(Users dbUser, Message message);
    /// <summary>
    /// 自动更新频道发布的多媒体组消息
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task OnMediaGroupChannelPostReceived(Users dbUser, Message message);
    /// <summary>
    /// 自动更新频道发布的文本消息
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task OnTextChannelPostReceived(Users dbUser, Message message);
}
