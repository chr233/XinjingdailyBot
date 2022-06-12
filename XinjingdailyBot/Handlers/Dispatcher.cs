using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers
{
    internal sealed class Dispatcher
    {
        internal static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Logger.Error(exception);
            return Task.CompletedTask;
        }

        internal static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Users? dbUser = await FetchUserHelper.FetchDbUser(update);

            if (dbUser == null)
            {
                await botClient.AutoReplyAsync(text: "不允许Bot访问", update, cancellationToken);
                return;
            }

            if (dbUser.IsBan)
            {
                await botClient.AutoReplyAsync("没有权限", update, cancellationToken);
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
                UpdateType.Message => BotOnMessageReceived(botClient, dbUser, update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, dbUser, update.EditedMessage!),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, dbUser, update.CallbackQuery!),
                UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, dbUser, update.InlineQuery!),
                UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, dbUser, update.ChosenInlineResult!),
                _ => UnknownUpdateHandlerAsync(botClient, dbUser, update)
            };

            try
            {
                await handler;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(botClient, ex, cancellationToken);
                await botClient.AutoReplyAsync(ex.Message, update, cancellationToken);
            }
        }

        internal static async Task BotOnMessageReceived(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            bool isMediaGroup = message.MediaGroupId != null;
            bool isPrivateChat = message.Chat.Type == ChatType.Private;

            switch (message.Type)
            {
                case MessageType.Text when message.Text!.StartsWith("/"):
                    string? answer = await Messages.CommandHandler.ExecCommand(botClient, dbUser, message);
                    await botClient.AutoReplyAsync(answer ?? "未知命令", message);
                    break;

                case MessageType.Text when isPrivateChat:
                    await Messages.PostHandler.HandleTextPosts(botClient, dbUser, message);
                    break;
                case MessageType.Photo when isMediaGroup && isPrivateChat:
                case MessageType.Audio when isMediaGroup && isPrivateChat:
                case MessageType.Video when isMediaGroup && isPrivateChat:
                case MessageType.Document when isMediaGroup && isPrivateChat:
                    await Messages.PostHandler.HandleMediaGroupPosts(botClient, dbUser, message);
                    break;

                case MessageType.Photo when isPrivateChat:
                case MessageType.Audio when isPrivateChat:
                case MessageType.Video when isPrivateChat:
                case MessageType.Document when isPrivateChat:
                    await Messages.PostHandler.HandleMediaPosts(botClient, dbUser, message);
                    break;
                default:
                    await botClient.AutoReplyAsync("未处理的消息", message);
                    break;
            }
        }

        private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, Users dbUser, CallbackQuery callbackQuery)
        {
            string? data = callbackQuery.Data;
            Message? message = callbackQuery.Message;
            if (string.IsNullOrEmpty(data) || message == null)
            {
                await botClient.AutoReplyAsync("Payload 非法", callbackQuery);
                await botClient.EditMessageReplyMarkupAsync(callbackQuery.InlineMessageId!);
                return;
            }

            string cmd = data[..4];

            switch (cmd)
            {
                case "post":
                    await Queries.PostHandler.HandleQuery(botClient, dbUser, callbackQuery);
                    break;

                case "revi":
                case "reje":
                    await Queries.ReviewHandler.HandleQuery();
                    break;
            }
        }

        private static async Task BotOnInlineQueryReceived(ITelegramBotClient botClient, Users dbUser, InlineQuery inlineQuery)
        {
            Console.WriteLine($"Received inline query from: {inlineQuery.From.Id}");

            InlineQueryResult[] results = {
            // displayed result
            new InlineQueryResultArticle(
                id: "3",
                title: "TgBots",
                inputMessageContent: new InputTextMessageContent(
                    "hello"
                )
            )
        };

            await botClient.AnswerInlineQueryAsync(inlineQueryId: inlineQuery.Id,
                                                   results: results,
                                                   isPersonal: true,
                                                   cacheTime: 0);
        }

        private static Task BotOnChosenInlineResultReceived(ITelegramBotClient botClient, Users dbUser, ChosenInlineResult chosenInlineResult)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId}");
            return Task.CompletedTask;
        }

        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Users dbUser, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }
    }

}
