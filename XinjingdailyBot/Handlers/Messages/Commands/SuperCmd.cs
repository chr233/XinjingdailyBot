using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Messages.Commands
{
    internal static class SuperCmd
    {
        internal static async Task<string> SetUserGroup(ITelegramBotClient botClient, Users dbUser, Message message, string[] args)
        {
            long? targetUserId = null;
            if (message.ReplyToMessage != null)
            {
                User? user = message.ReplyToMessage.From;
                if (user != null)
                {
                    targetUserId = user.Id;
                }
            }
            else
            {
                foreach (string arg in args)
                {
                    if (arg.StartsWith('@'))
                    {
                        string userName = arg[1..];

                        Users? user = await DB.Queryable<Users>().FirstAsync(x => x.UserName == userName);

                        if (user != null)
                        {
                            targetUserId = user.UserID;
                            break;
                        }
                    }
                    else
                    {
                        if (long.TryParse(arg, out var uid))
                        {
                            targetUserId = uid;
                            break;
                        }
                    }
                }
            }

            if (targetUserId == null)
            {
                return "找不到目标用户";
            }

            var keyboard = MarkupHelper.SetGroupKeyboard();
            var msg = await botClient.SendTextMessageAsync(message.Chat.Id, "请选择用户组", replyMarkup: keyboard, replyToMessageId: message.MessageId, allowSendingWithoutReply: true);

            CmdRecord record = new()
            {
                ChatID = msg.Chat.Id,
                MessageID = msg.MessageId,
                UserID = dbUser.UserID,
                Command = "SETGROUP",
                TargetUserID = (long)targetUserId,
            };

            await DB.Insertable(record).ExecuteCommandAsync();

            return "";
        }

        internal static async Task<string> ResponseRestart(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            CmdRecord record = new()
            {
                ChatID = message.Chat.Id,
                MessageID = message.MessageId,
                UserID = dbUser.UserID,
                Command = "RESTART",
            };

            await DB.Insertable(record).ExecuteCommandAsync();

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

            return "Bot即将重启";
        }

    }
}
