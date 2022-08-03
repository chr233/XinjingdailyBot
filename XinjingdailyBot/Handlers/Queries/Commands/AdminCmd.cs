using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;

namespace XinjingdailyBot.Handlers.Queries.Commands
{
    internal static class AdminCmd
    {
        /// <summary>
        /// 搜索用户
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task ResponseSearchUser(ITelegramBotClient botClient, Users dbUser, CallbackQuery callbackQuery, string[] args)
        {
            async Task<(string, InlineKeyboardMarkup?)> exec()
            {
                if (args.Length < 3)
                {
                    return ("参数有误", null);
                }

                string query = args[1];

                if (!int.TryParse(args[2], out int page))
                {
                    page = 1;
                }

                return await FetchUserHelper.QueryUserList(dbUser, query, page);
            }
            (string text, var kbd) = await exec();
            await botClient.EditMessageTextAsync(callbackQuery.Message!, text, ParseMode.Html, true, kbd);
        }
    }
}
