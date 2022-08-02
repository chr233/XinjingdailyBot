using NLog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Models;

namespace XinjingdailyBot.Helpers
{
    internal static class LoggerHelper
    {
        internal static Logger Logger { get; private set; } = LogManager.GetLogger(SharedInfo.XJBBot);

        /// <summary>
        /// 输出update
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="update"></param>
        /// <param name="dbUser"></param>
        internal static void LogUpdate(this Logger logger, Update update, Users dbUser)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                case UpdateType.EditedMessage:
                    Logger.LogMessage(update.Message!, dbUser);
                    break;
                case UpdateType.CallbackQuery:
                    Logger.LogCallbackQuery(update.CallbackQuery!, dbUser);
                    break;
                default:
                    Logger.Debug($"U 未知消息 {dbUser} {update.Type}");
                    break;
            }
        }

        /// <summary>
        /// 输出message
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="dbUser"></param>
        internal static void LogMessage(this Logger logger, Message message, Users dbUser)
        {
            string content = message.Type switch
            {
                MessageType.Text => $"[文本] {message.Text}",
                MessageType.Photo => $"[图片] {message.Caption}",
                MessageType.Audio => $"[音频] {message.Caption}",
                MessageType.Video => $"[视频] {message.Caption}]",
                MessageType.Voice => $"[语音] {message.Caption}]",
                MessageType.Document => $"[文件] {message.Caption}]",
                MessageType.Sticker => $"[贴纸] {message.Sticker!.SetName}]",
                _ => "[其他消息]",
            };

            var chat = message.Chat;

            string chatFrom = chat.Type switch
            {
                ChatType.Private => $"私聊",
                ChatType.Group => $"群组-{chat.Title}",
                ChatType.Channel => $"频道-{chat.Title}",
                ChatType.Supergroup => $"群组-{chat.Title}",
                _ => $"未知-{chat.Title}",
            };

            Logger.Debug($"M {chatFrom} {dbUser} {content}");
        }

        /// <summary>
        /// 输出query
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="query"></param>
        /// <param name="dbUser"></param>
        internal static void LogCallbackQuery(this Logger logger, CallbackQuery callbackQuery, Users dbUser)
        {
            Logger.Debug($"Q {callbackQuery.Id} {dbUser} [数据] {callbackQuery.Data}");
        }
    }
}
