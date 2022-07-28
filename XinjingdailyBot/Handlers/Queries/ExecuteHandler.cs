using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Models;

namespace XinjingdailyBot.Handlers.Queries
{
    internal class ExecuteHandler
    {
        /// <summary>
        /// 处理CallbackQuery
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        internal static async Task HandleQuery(ITelegramBotClient botClient, Users dbUser, CallbackQuery callbackQuery)
        {
            Message message = callbackQuery.Message!;


        }
    }
}
