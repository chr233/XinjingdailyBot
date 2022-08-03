using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace XinjingdailyBot.Helpers
{
    internal static class BotClientHelper
    {
        /// <summary>
        /// 自动选择回复方式
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="text"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal static async Task<Message?> AutoReplyAsync(
            this ITelegramBotClient botClient,
            string text,
            Update update,
            CancellationToken cancellationToken = default)
        {
            if (update.Type == UpdateType.Message)
            {
                Message msg = update.Message!;
                return await botClient.SendTextMessageAsync(msg.Chat.Id, text, replyToMessageId: msg.MessageId, allowSendingWithoutReply: true, cancellationToken: cancellationToken);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                CallbackQuery query = update.CallbackQuery!;
                await botClient.AnswerCallbackQueryAsync(query.Id, text, cancellationToken: cancellationToken);
            }
            return null;
        }

        /// <summary>
        /// 发送回复
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="text"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal static async Task<Message> AutoReplyAsync(
            this ITelegramBotClient botClient,
            string text,
            Message message,
            ParseMode? parsemode = null,
            CancellationToken cancellationToken = default)
        {
            return await botClient.SendTextMessageAsync(message.Chat.Id, text, parsemode, replyToMessageId: message.MessageId, allowSendingWithoutReply: true, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 发送回复
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="text"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal static async Task AutoReplyAsync(
            this ITelegramBotClient botClient,
            string text,
            CallbackQuery query,
            CancellationToken cancellationToken = default)
        {
            await botClient.AnswerCallbackQueryAsync(query.Id, text, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 编辑消息Markup
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <param name="replyMarkup"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal static async Task<Message> EditMessageReplyMarkupAsync(
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
        /// <param name="replyMarkup"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal static async Task RemoveMessageReplyMarkupAsync(
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
        internal static async Task<Message> EditMessageTextAsync(
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
    }
}
