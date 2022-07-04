using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;

namespace XinjingdailyBot.Handlers.Messages.Commands
{
    internal static class AdminCmd
    {

        internal static async Task<string> ResponseNo(ITelegramBotClient botClient, Users dbUser, Message message, string reason)
        {
            if (string.IsNullOrEmpty(reason.Trim()))
            {
                await botClient.AutoReplyAsync("请输入拒绝理由", message);
                return "";
            }


            return "";
        }
    }
}
