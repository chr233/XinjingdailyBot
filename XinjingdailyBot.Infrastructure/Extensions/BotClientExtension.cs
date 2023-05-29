using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace XinjingdailyBot.Infrastructure.Extensions
{
    public static class BotClientExtension
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 发送回复
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="text"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<Message> AutoReplyAsync(
            this ITelegramBotClient botClient,
            string text,
            Message message,
            ParseMode? parsemode = null,
            CancellationToken cancellationToken = default)
        {
            return await botClient.SendTextMessageAsync(message.Chat.Id, text, parseMode: parsemode, replyToMessageId: message.MessageId, allowSendingWithoutReply: true, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 发送回复
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="text"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task AutoReplyAsync(
            this ITelegramBotClient botClient,
            string text,
            CallbackQuery query,
            bool showAlert = false,
            CancellationToken cancellationToken = default)
        {
            await botClient.AnswerCallbackQueryAsync(query.Id, text, showAlert: showAlert, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 编辑消息Markup
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <param name="replyMarkup"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<Message> EditMessageReplyMarkupAsync(
            this ITelegramBotClient botClient,
            Message message,
            InlineKeyboardMarkup? replyMarkup = default,
            CancellationToken cancellationToken = default)
        {
            return await botClient.EditMessageReplyMarkupAsync(message.Chat.Id, message.MessageId, replyMarkup, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 删除消息Markup
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task RemoveMessageReplyMarkupAsync(
            this ITelegramBotClient botClient,
            Message message,
            CancellationToken cancellationToken = default)
        {
            await botClient.EditMessageReplyMarkupAsync(message.Chat.Id, message.MessageId, null, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 编辑消息
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <param name="text"></param>
        /// <param name="replyMarkup"></param>
        /// <param name="parseMode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<Message> EditMessageTextAsync(
            this ITelegramBotClient botClient,
            Message message,
            string text,
            ParseMode? parseMode = default,
            bool? disableWebPagePreview = null,
            InlineKeyboardMarkup? replyMarkup = default,

            CancellationToken cancellationToken = default)
        {
            return await botClient.EditMessageTextAsync(message.Chat.Id, message.MessageId, text, parseMode: parseMode, disableWebPagePreview: disableWebPagePreview, replyMarkup: replyMarkup, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 发送命令回复
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="text"></param>
        /// <param name="message"></param>
        /// <param name="autoDelete">私聊始终不删除消息, 群聊中默认删除消息, 但可以指定不删除</param>
        /// <param name="parsemode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<Message> SendCommandReply(
            this ITelegramBotClient botClient,
            string text,
            Message message,
            bool? autoDelete = null,
            ParseMode? parsemode = null,
            IReplyMarkup? replyMarkup = null,
            CancellationToken cancellationToken = default)
        {
            //私聊始终不删除消息, 群聊中默认删除消息, 但可以指定不删除
            bool delete = (autoDelete != null ? autoDelete.Value : (message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup)) && message.Chat.Type != ChatType.Private;

            var msg = await botClient.SendTextMessageAsync(message.Chat.Id, text, parseMode: parsemode, replyToMessageId: message.MessageId, replyMarkup: replyMarkup, disableWebPagePreview: true, allowSendingWithoutReply: true, cancellationToken: cancellationToken);

            if (delete)
            {
                _ = Task.Run(async () => {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    try
                    {
                        await botClient.DeleteMessageAsync(msg.Chat.Id, msg.MessageId, cancellationToken);
                    }
                    catch
                    {
                        _logger.Error("删除消息 {messageId} 失败", msg.MessageId);
                    }
                }, cancellationToken);
            }

            return msg;
        }
    }
}
