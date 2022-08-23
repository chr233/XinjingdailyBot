using XinjingdailyBot.Helpers;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Queries
{
    internal static class QueryDispatcher
    {

        /// <summary>
        /// 忽略旧的CallbackQuery
        /// </summary>
        private static TimeSpan IgnoreQueryOlderThan { get; } = TimeSpan.FromSeconds(30);

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
            if (message == null)
            {
                await botClient.AutoReplyAsync("消息不存在", callbackQuery);
                return;
            }

            string? data = callbackQuery.Data;
            if (string.IsNullOrEmpty(data))
            {
                await botClient.RemoveMessageReplyMarkupAsync(message);
                return;
            }

            Logger.LogCallbackQuery(callbackQuery, dbUser);

            //忽略过旧的Query
            if (DateTime.Now - message.Date > IgnoreQueryOlderThan)
            {
                //return;
            }

            string[] args = data.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
            if (!args.Any()) { return; }

            string cmd = args.First();
            args = args[1..];

            switch (cmd)
            {
                //投稿确认
                case "post":
                    await PostHandler.HandleQuery(botClient, dbUser, callbackQuery);
                    break;

                //审核相关
                case "review":
                case "reject":
                    await ReviewHandler.HandleQuery(botClient, dbUser, callbackQuery);
                    break;

                //命令回调
                case "cmd":
                    await CommandHandler.HandleQuery(botClient, dbUser, callbackQuery, args);
                    break;

                //取消操作
                case "cancel":
                    await botClient.AutoReplyAsync("操作已取消", callbackQuery);
                    await botClient.EditMessageTextAsync(message, "操作已取消", replyMarkup: null);
                    break;

                //无动作
                case "none":
                    await botClient.AutoReplyAsync("无", callbackQuery);
                    break;

                default:
                    break;
            }
        }
    }
}
