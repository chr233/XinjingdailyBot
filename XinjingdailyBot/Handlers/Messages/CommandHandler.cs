using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Handlers.Messages.Commands;
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

            string answer = await ExecCommand(botClient, dbUser, message) ?? "未知命令";

            if (!string.IsNullOrEmpty(answer))
            {
                if (inGroup && !message.Text!.Contains(BotName) && answer == "未知命令")
                {
                    return;
                }

                var msg = await botClient.SendTextMessageAsync(chatID, answer, ParseMode.Html, replyToMessageId: message.MessageId, allowSendingWithoutReply: true);

                if (inGroup) //定时删除命令消息
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

            if (!args.Any())
            {
                return null;
            }

            switch (args.Length)
            {
                case 0:
                    return null;

                case 1://不带参数
                    switch (args[0].ToUpperInvariant())
                    {
                        case "VERSION":
                            return NormalCmd.ResponseVersion();
                        case "START":
                            return NormalCmd.ResponseStart();

                        //Normal
                        case "HELP" when normal:
                            return NormalCmd.ResponseHelp(dbUser);

                        case "ANYMOUSE" when normal:
                        case "ANONYMOUS" when normal:
                            return await NormalCmd.ResponseAnymouse(dbUser);

                        case "NOTIFICATION" when normal:
                            return await NormalCmd.ResponseNotification(dbUser);

                        case "MYINFO" when normal:
                            return NormalCmd.ResponseMyInfo(dbUser);

                        case "MYRIGHT" when normal:
                            return NormalCmd.ResponseMyRight(dbUser);

                        case "ADMIN" when normal:
                        case "ADMINS" when normal:
                            return await NormalCmd.ResponseCallAdmins(botClient, dbUser, message);

                        case "PING" when normal:
                            return NormalCmd.ResponsePing();

                        //Admin
                        case "REVIEWHELP" when admin:
                            return await AdminCmd.ResponseReviewHelp(dbUser);

                        case "NO" when admin:
                            return await AdminCmd.ResponseNo(botClient, dbUser, message, null);
                        //case "YES" when admin:
                        //    return await AdminCmd.ResponseYes(botClient, dbUser, message, null);

                        case "GROUPINFO" when admin:
                            return AdminCmd.ResponseGroupInfo(dbUser, message);


                        case "QUERYBAN" when admin:
                            return await AdminCmd.QueryBan(botClient, dbUser, message);

                        //Super
                        case "SETGROUP" when super:
                            return await SuperCmd.SetUserGroup(botClient, dbUser, message, null);

                        case "RESTART" when super:
                            return await SuperCmd.ResponseRestart(botClient, dbUser, message);

                        default:
                            return null;
                    }
                default://带参数
                    int argsLen = args.Length;

                    string payload = string.Join(" ", args[1..]);

                    switch (args[0].ToUpperInvariant())
                    {
                        //Admin
                        case "REVIEWHELP" when admin:
                            return await AdminCmd.ResponseReviewHelp(dbUser);

                        case "NO" when admin:
                            return await AdminCmd.ResponseNo(botClient, dbUser, message, payload);

                        case "BAN" when admin:
                            return await AdminCmd.ResponseBan(botClient, dbUser, message, args[1..]);

                        case "UNBAN" when admin:
                            return await AdminCmd.ResponseUnban(botClient, dbUser, message, args[1..]);

                        case "QUERYBAN":
                            return await AdminCmd.QueryBan(botClient, dbUser, message, args[1..]);

                        //case "YES" when admin:
                        //    return await AdminCmd.ResponseYes(botClient, dbUser, message, payload);

                        //Super
                        case "SETGROUP" when super:
                            return await SuperCmd.SetUserGroup(botClient, dbUser, message, args[1..]);

                        default:
                            return null;
                    }
            }
        }
    }
}
