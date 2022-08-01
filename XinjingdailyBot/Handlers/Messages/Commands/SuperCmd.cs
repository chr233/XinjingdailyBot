using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Messages.Commands
{
    internal static class SuperCmd
    {
        /// <summary>
        /// 机器人重启
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task ResponseRestart(ITelegramBotClient botClient, Message message)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    Process.Start(Environment.ProcessPath!);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                await Task.Delay(2000);

                Environment.Exit(0);
            });

            string text = "机器人即将重启";
            await botClient.SendCommandReply(text, message);
        }

        /// <summary>
        /// 设置用户组
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task SetUserGroup(ITelegramBotClient botClient, Users dbUser, Message message, string[] args)
        {
            async Task<(string, InlineKeyboardMarkup?)> exec()
            {
                var targetUser = await FetchUserHelper.FetchTargetUser(message);

                if (targetUser == null)
                {
                    if (args.Any())
                    {
                        targetUser = await FetchUserHelper.FetchTargetUser(args.First());
                    }
                }

                if (targetUser == null)
                {
                    return ("找不到指定用户", null);
                }

                if (targetUser.Id == dbUser.Id)
                {
                    return ("无法对自己进行操作", null);
                }

                if (targetUser.GroupID >= dbUser.GroupID)
                {
                    return ("无法对同级管理员进行此操作", null);
                }

                var keyboard = await MarkupHelper.SetUserGroupKeyboard(dbUser, targetUser);
                return ("请选择新的用户组", keyboard);
            }

            (string text, InlineKeyboardMarkup? kbd) = await exec();
            await botClient.SendCommandReply(text, message, autoDelete: false, replyMarkup: kbd);
        }
    }
}
