using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace XinjingdailyBot.Infrastructure.Extensions
{
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
            string content = message.Type switch
            {
                MessageType.Text => $"[文本] {message.Text}",
                MessageType.Photo => $"[图片] {message.Caption}",
                MessageType.Audio => $"[音频] {message.Caption}",
                MessageType.Video => $"[视频] {message.Caption}",
                MessageType.Voice => $"[语音] {message.Caption}",
                MessageType.Document => $"[文件] {message.Caption}",
                MessageType.Sticker => $"[贴纸] {message.Sticker!.SetName}",
                MessageType.MessagePinned => $"[消息置顶] {message.PinnedMessage!.Text}",
                MessageType.ChatMemberLeft => $"[成员退出] {message.LeftChatMember!.UserProfile()}",
                MessageType.ChatMembersAdded => $"[成员加入] {message.NewChatMembers?.First().UserProfile()} 等 {message.NewChatMembers?.Length ?? 0} 人",
                MessageType.ChatTitleChanged => $"[群名变更] {message.NewChatTitle}",
                MessageType.ChatPhotoChanged => $"[群头像变更]",
                MessageType.ChatPhotoDeleted => $"[群头像删除]",
                MessageType.MigratedToSupergroup => $"[群升级为超级群]",
                MessageType.MigratedFromGroup => $"[超级群降级为群]",
                MessageType.Poll => $"[投票] {message.Poll!.Question}",
                _ => $"[其他] {message.Type}",
            };

            var chat = message.Chat;

            string chatFrom = chat.Type switch
            {
                ChatType.Private => "【私聊】",
                ChatType.Group => $"【群组|{chat.Title}】",
                ChatType.Channel => $"【频道|{chat.Title}】",
                ChatType.Supergroup => $"【群组|{chat.Title}】",
                _ => $"【未知|{chat.Title}】",
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
            logger.LogInformation("【回调|{Id}】 {user} {Data}", callbackQuery.Id, user, callbackQuery.Data);
        }

        /// <summary>
        /// 输出query
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="inlineQuery"></param>
        public static void LogInlineQuery(this ILogger logger, InlineQuery inlineQuery)
        {
            string user = inlineQuery.From.UserToString();
            logger.LogDebug("【查询|{Id}】 {user} {Data}", inlineQuery.Id, user, inlineQuery.Query);
        }
    }
}
