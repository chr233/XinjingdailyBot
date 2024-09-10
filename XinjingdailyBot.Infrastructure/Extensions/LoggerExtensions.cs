using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace XinjingdailyBot.Infrastructure.Extensions;

/// <summary>
/// Logger扩展
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// 输出update
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="update"></param>
    public static void LogUpdate(this ILogger logger, Update update)
    {
        switch (update.Type)
        {
            case UpdateType.ChannelPost:
                logger.LogMessage(update.ChannelPost!);
                break;
            case UpdateType.EditedChannelPost:
                logger.LogMessage(update.EditedChannelPost!);
                break;
            case UpdateType.Message:
                logger.LogMessage(update.Message!);
                break;
            case UpdateType.EditedMessage:
                logger.LogMessage(update.EditedMessage!);
                break;
            case UpdateType.CallbackQuery:
                logger.LogCallbackQuery(update.CallbackQuery!);
                break;
            case UpdateType.InlineQuery:
                logger.LogInlineQuery(update.InlineQuery!);
                break;
            case UpdateType.MyChatMember:
                logger.LogMyChatMember(update.MyChatMember!);
                break;
            default:
                logger.LogDebug("U 未知消息 {Type}", update.Type);
                break;
        }
    }

    /// <summary>
    /// 输出message
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="message"></param>
    public static void LogMessage(this ILogger logger, Message message)
    {
        string content = message.Type switch {
            MessageType.Text => $"[文本] {message.Text}",
            MessageType.Photo => $"[图片] {message.Caption}",
            MessageType.Audio => $"[音频] {message.Caption}",
            MessageType.Video => $"[视频] {message.Caption}",
            MessageType.Voice => $"[语音] {message.Caption}",
            MessageType.Document => $"[文件] {message.Caption}",
            MessageType.Sticker => $"[贴纸] {message.Sticker!.SetName}",
            MessageType.PinnedMessage => $"[消息置顶] {message.PinnedMessage!.Text}",
            MessageType.LeftChatMember => $"[成员退出] {message.LeftChatMember!.UserProfile()}",
            MessageType.NewChatMembers => $"[成员加入] {message.NewChatMembers?.First().UserProfile()} 等 {message.NewChatMembers?.Length ?? 0} 人",
            MessageType.NewChatTitle => $"[群名变更] {message.NewChatTitle}",
            MessageType.NewChatPhoto => $"[群头像变更]",
            MessageType.DeleteChatPhoto => $"[群头像删除]",
            MessageType.Poll => $"[投票] {message.Poll!.Question}",
            _ => $"[其他] {message.Type}",
        };

        var chat = message.Chat;

        string chatFrom = chat.Type switch {
            ChatType.Private => $"[私聊|{chat.FirstName}{chat.LastName}]",
            ChatType.Group => $"[群组|{chat.Title}]",
            ChatType.Channel => $"[频道|{chat.Title}]",
            ChatType.Supergroup => $"[群组|{chat.Title}]",
            ChatType.Sender => $"[发送者|{chat.Title}]",
            _ => $"[未知|{chat.Title}]",
        };

        string user = message.From?.UserToString() ?? "未知";

        logger.LogInformation("{chatFrom} {user} {content}", chatFrom, user, content);
    }

    /// <summary>
    /// 输出query
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="callbackQuery"></param>
    public static void LogCallbackQuery(this ILogger logger, CallbackQuery callbackQuery)
    {
        string user = callbackQuery.From.UserToString();
        logger.LogInformation("[回调|{Id}] {user} {Data}", callbackQuery.Id, user, callbackQuery.Data);
    }

    /// <summary>
    /// 输出query
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="inlineQuery"></param>
    public static void LogInlineQuery(this ILogger logger, InlineQuery inlineQuery)
    {
        string user = inlineQuery.From.UserToString();
        logger.LogDebug("[查询|{Id}] {user} {Data}", inlineQuery.Id, user, inlineQuery.Query);
    }

    /// <summary>
    /// 输出MyChatMember
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="chatMemberUpdated"></param>
    public static void LogMyChatMember(this ILogger logger, ChatMemberUpdated chatMemberUpdated)
    {
        var chat = chatMemberUpdated.Chat;
        string chatFrom = chat.Type switch {
            ChatType.Private => $"[私聊|{chat.FirstName}{chat.LastName}]",
            ChatType.Group => $"[群组|{chat.Title}]",
            ChatType.Channel => $"[频道|{chat.Title}]",
            ChatType.Supergroup => $"[群组|{chat.Title}]",
            ChatType.Sender => $"[发送者|{chat.Title}]",
            _ => $"[未知|{chat.Title}]",
        };

        var op = chatMemberUpdated.From.UserToString();
        var user = chatMemberUpdated.NewChatMember.User.UserToString();

        var oldAction = chatMemberUpdated.OldChatMember.GetMemberAction();
        var newAction = chatMemberUpdated.NewChatMember.GetMemberAction();

        if (op != user)
        {
            logger.LogDebug("[成员变更] {chatFrom} 用户 {op} 设置 {user} {old}->{new}", chatFrom, op, user, oldAction, newAction);
        }
        else
        {
            logger.LogDebug("[成员变更] {chatFrom} 用户 {user} {old}->{new}", chatFrom, user, oldAction, newAction);
        }
    }

    private static string GetMemberAction(this ChatMember chatMember)
    {
        return chatMember.Status switch {
            ChatMemberStatus.Creator => "所有者",
            ChatMemberStatus.Administrator => "管理员",
            ChatMemberStatus.Member => "普通成员",
            ChatMemberStatus.Left => "退出",
            ChatMemberStatus.Kicked => "被踢出",
            ChatMemberStatus.Restricted => "被限制",
            _ => "未知",
        };
    }
}
