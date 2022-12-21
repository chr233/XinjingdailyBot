using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace XinjingdailyBot.Infrastructure.Extensions
{
    public static class LoggerExtensions
    {
        private static readonly ILogger _logger = (ILogger)NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 输出update
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="update"></param>
        /// <param name="dbUser"></param>
        public static void LogUpdate(this ILogger logger, Update update)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                case UpdateType.EditedMessage:
                    _logger.LogMessage(update.Message!);
                    break;
                case UpdateType.CallbackQuery:
                    _logger.LogCallbackQuery(update.CallbackQuery!);
                    break;
                default:
                    _logger.LogDebug($"U 未知消息 {update.Type}");
                    break;
            }
        }

        /// <summary>
        /// 输出message
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="dbUser"></param>
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
                _ => $"[其他] {message.Type}",
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

            _logger.LogInformation($"M {chatFrom}  {content}");
        }

        /// <summary>
        /// 输出query
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dbUser"></param>
        public static void LogCallbackQuery(this ILogger logger, CallbackQuery callbackQuery)
        {
            _logger.LogDebug($"Q {callbackQuery.Id}  [数据] {callbackQuery.Data}");
        }
    }
}
