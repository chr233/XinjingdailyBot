using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Queries
{
    internal static class QueryDispatcher
    {
        /// <summary>
        /// 处理CallbackQuery
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        internal static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, Users dbUser, CallbackQuery callbackQuery)
        {
            //检查是否封禁
            if (dbUser.IsBan)
            {
                await botClient.AutoReplyAsync("无权访问", callbackQuery);
                return;
            }

            Message? message = callbackQuery.Message;
            string? data = callbackQuery.Data;

            if (message == null || string.IsNullOrEmpty(data))
            {
                await botClient.AutoReplyAsync("Payload 非法", callbackQuery);
                await botClient.EditMessageReplyMarkupAsync(callbackQuery.InlineMessageId!);
                return;
            }

            if (BotConfig.Debug)
            {
                Logger.Debug($"Q {callbackQuery.Data} {dbUser}");
            }

            string[] args = data.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
            if (!args.Any()) { return; }
            string cmd = args.First();

            switch (cmd)
            {
                case "post":
                    await PostHandler.HandleQuery(botClient, dbUser, callbackQuery);
                    break;

                case "review":
                case "reject":
                    await ReviewHandler.HandleQuery(botClient, dbUser, callbackQuery);
                    break;

                

                default:
                    break;
            }
        }
    }
}
