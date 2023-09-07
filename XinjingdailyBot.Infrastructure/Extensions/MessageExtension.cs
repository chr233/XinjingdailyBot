using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace XinjingdailyBot.Infrastructure.Extensions;

/// <summary>
/// Message扩展
/// </summary>
public static class MessageExtension
{
    /// <summary>
    /// 是否可以遮罩
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static bool CanSpoiler(this Message message)
    {
        return message.Type == MessageType.Photo || message.Type == MessageType.Video || message.Type == MessageType.Animation;
    }

    /// <summary>
    /// 获取消息链接
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static string GetMessageLink(this Message message)
    {
        var username = message.Chat.Username ?? message.Chat.Id.ToString();
        return $"https://t.me/{username}/{message.MessageId}";
    }
}
