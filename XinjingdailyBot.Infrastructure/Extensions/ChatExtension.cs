using Telegram.Bot.Types;

namespace XinjingdailyBot.Infrastructure.Extensions;

/// <summary>
/// Chat扩展
/// </summary>
public static class ChatExtension
{
    /// <summary>
    /// 获取ChatID
    /// </summary>
    /// <param name="chat"></param>
    /// <returns></returns>
    public static string ChatID(this Chat chat)
    {
        return string.IsNullOrEmpty(chat.Username) ? $"#{chat.Id}" : $"@{chat.Username}";
    }

    /// <summary>
    /// 获取完整ChatID
    /// </summary>
    /// <param name="chat"></param>
    /// <returns></returns>
    public static string FullChatID(this Chat chat)
    {
        return string.IsNullOrEmpty(chat.Username) ? $"#{chat.Id}" : $"@{chat.Username} #{chat.Id}";
    }

    /// <summary>
    /// 获取Chat资料
    /// </summary>
    /// <param name="chat"></param>
    /// <returns></returns>
    public static string ChatProfile(this Chat chat)
    {
        return $"{chat.Title} {chat.ChatID()}";
    }

    /// <summary>
    /// 获取完整Chat资料
    /// </summary>
    /// <param name="chat"></param>
    /// <returns></returns>
    public static string FullChatProfile(this Chat chat)
    {
        return $"{chat.Title} {chat.FullChatID()}";
    }

    /// <summary>
    /// HTML转义后的聊天名
    /// </summary>
    /// <param name="chat"></param>
    /// <returns></returns>
    public static string EscapedChatName(this Chat chat)
    {
        return chat.Title?.EscapeHtml() ?? "";
    }

    /// <summary>
    /// 获取消息链接
    /// </summary>
    /// <param name="chat"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
    public static string GetMessageLink(this Chat chat, long messageId)
    {
        string link = !string.IsNullOrEmpty(chat.Username) ? $"https://t.me/{chat.Username}/{messageId}" : $"https://t.me/c/{chat.Id}/{messageId}";
        return link;
    }
}
