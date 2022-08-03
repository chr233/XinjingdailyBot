using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Queries.Commands
{
    internal static class NormalCmd
    {
        /// <summary>
        /// 显示命令回复
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task ResponseSay(ITelegramBotClient botClient, Users dbUser, CallbackQuery callbackQuery, string[] args)
        {
            string exec()
            {
                if (args.Length < 2)
                {
                    return "参数有误";
                }
                return string.Join(' ', args[1..]);
            }
            string text = exec();
            await botClient.AutoReplyAsync(text,callbackQuery);
        }
    }
}
