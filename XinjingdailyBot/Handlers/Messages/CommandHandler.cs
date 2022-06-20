using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Messages
{
    internal class CommandHandler
    {
        /// <summary>
        /// 响应命令
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task HandleCommand(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            long chatID = message.Chat.Id;

            bool inGroup = message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup;

            string? answer = await ExecCommand(botClient, dbUser, message) ?? "未知命令";

            if (!string.IsNullOrEmpty(answer))
            {
                if (inGroup && !message.Text!.Contains(BotName) && answer == "未知命令")
                {
                    return;
                }

                if (!string.IsNullOrEmpty(answer))
                {
                    var msg = await botClient.SendTextMessageAsync(chatID, answer, ParseMode.Html, replyToMessageId: inGroup ? null : message.MessageId, allowSendingWithoutReply: true);

                    if (inGroup)
                    {
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromSeconds(30));
                            await botClient.DeleteMessageAsync(chatID, message.MessageId);
                            await botClient.DeleteMessageAsync(chatID, msg.MessageId);
                        });
                    }
                }

            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static async Task<string?> ExecCommand(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            string input = message.Text![1..];

            int index = input.IndexOf('@');

            if (index != -1)
            {
                string botName = input[(index + 1)..];
                if (!botName.Equals(BotName, StringComparison.OrdinalIgnoreCase))
                {
                    return "";
                }
                input = input[..index];
            }

            bool super = dbUser.Right.HasFlag(UserRights.SuperCmd);
            bool admin = dbUser.Right.HasFlag(UserRights.AdminCmd) || super;
            bool normal = dbUser.Right.HasFlag(UserRights.NormalCmd) || admin;

            string[] args = input.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);

            switch (args.Length)
            {
                case 0:
                    return null;

                case 1://不带参数
                    switch (args[0].ToUpperInvariant())
                    {
                        case "VERSION":
                            return Commands.NormalCmd.ResponseVersion();

                        case "START" when normal:
                        case "HELP" when normal:
                            return Commands.NormalCmd.ResponseHelp(dbUser);

                        case "ANYMOUSE" when normal:
                            return await Commands.NormalCmd.ResponseAnymouse(dbUser);

                        case "NOTIFICATION" when normal:
                            return await Commands.NormalCmd.ResponseNotification(dbUser);

                        case "MYINFO" when normal:
                            return Commands.NormalCmd.ResponseMyInfo(dbUser);

                        case "MYRIGHT" when normal:
                            return Commands.NormalCmd.ResponseMyRight(dbUser);

                        case "ADMIN" when normal:
                        case "ADMINS" when normal:
                            return await Commands.NormalCmd.ResponseCallAdmins(botClient, dbUser, message);

                        case "SETGROUP" when super:
                            return await Commands.AdminCmd.SetUserGroup(botClient, dbUser, message, args[1..]);

                        case "RESTART" when super:
                            return await Commands.AdminCmd.Restart(botClient, dbUser, message);

                        default:
                            return null;
                    }
                default://带参数
                    switch (args[0].ToUpperInvariant())
                    {

                        default:
                            return null;
                    }
            }
        }
    }
}
