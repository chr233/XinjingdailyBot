using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers
{
    internal static class UpdateDispatcher
    {
        /// <summary>
        /// Update消息处理器
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Users? dbUser = await FetchUserHelper.FetchDbUser(update);

            if (dbUser == null)
            {
                return;
            }

            var handler = update.Type switch
            {
                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                UpdateType.Message => Messages.MessageDispatcher.BotOnMessageReceived(botClient, dbUser, update.Message!),
                UpdateType.EditedMessage => Messages.MessageDispatcher.BotOnMessageReceived(botClient, dbUser, update.EditedMessage!),
                UpdateType.CallbackQuery => Queries.QueryDispatcher.BotOnCallbackQueryReceived(botClient, dbUser, update.CallbackQuery!),
                //UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, dbUser, update.InlineQuery!),
                //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, dbUser, update.ChosenInlineResult!),
                _ => UnknownUpdateHandlerAsync(botClient, dbUser, update)
            };

            try
            {
                await handler;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(botClient, ex, cancellationToken);
                if (IsDebug)
                {
                    await botClient.AutoReplyAsync(ex.Message, update, cancellationToken);
                }
            }
        }

        /// <summary>
        /// 其他消息处理器
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Users dbUser, Update update)
        {
            Logger.LogUpdate(update, dbUser);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 错误处理器
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="exception"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Logger.Error(exception);
            return Task.CompletedTask;
        }
    }
}
