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

            List<Message> replyMessages = new() { message };

            await ExecCommand(botClient, dbUser, message, replyMessages);

            //定时删除群组中的命令消息
            if (inGroup)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    foreach (var msg in replyMessages)
                    {
                        try
                        {
                            await botClient.DeleteMessageAsync(chatID, msg.MessageId);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message">用户消息原文</param>
        /// <param name="replyMessages">回复的消息,用于自动删除</param>
        /// <returns></returns>
        private static async Task ExecCommand(ITelegramBotClient botClient, Users dbUser, Message message, List<Message> replyMessages)
        {
            string input = message.Text![1..];

            string[] args = input.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);

            if (!args.Any()) { return; }

            string cmd = args.First();
            args = args[1..];
            string payload = string.Join(' ', args);

            //判断是不是自己的命令
            int index = cmd.IndexOf('@');
            if (index != -1)
            {
                string botName = cmd[(index + 1)..];
                if (!botName.Equals(BotName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                cmd = cmd[..index];
            }

            bool super = dbUser.Right.HasFlag(UserRights.SuperCmd);
            bool admin = dbUser.Right.HasFlag(UserRights.AdminCmd) || super;
            bool normal = dbUser.Right.HasFlag(UserRights.NormalCmd) || admin;

            //响应命令
            switch (cmd.ToUpperInvariant())
            {
                //Common - 通用命令, 不鉴权
                case "HELP" when normal:
                    await CommonCmd.ResponseHelp(botClient, dbUser, message, replyMessages);
                    return;

                case "START":
                    await CommonCmd.ResponseStart(botClient, dbUser, message, replyMessages);
                    return;

                case "VERSION":
                    await CommonCmd.ResponseVersion(botClient, message, replyMessages);
                    return;

                case "MYBAN":
                    await CommonCmd.ResponseMyBan(botClient, dbUser, message, replyMessages);
                    return;

                //Normal - 普通命令
                case "PING" when normal:
                    await NormalCmd.ResponsePing(botClient, message, replyMessages);
                    return;

                case "ANONYMOUS" when normal:
                    await NormalCmd.ResponseAnymouse(botClient, dbUser, message, replyMessages);
                    return;

                case "NOTIFICATION" when normal:
                    await NormalCmd.ResponseNotification(botClient, dbUser, message, replyMessages);
                    return;

                case "MYINFO" when normal:
                    await NormalCmd.ResponseMyInfo(botClient, dbUser, message, replyMessages);
                    return;

                case "MYRIGHT" when normal:
                    await NormalCmd.ResponseMyRight(botClient, dbUser, message, replyMessages);
                    return;

                case "ADMIN" when normal:
                case "ADMINS" when normal:
                    await NormalCmd.ResponseCallAdmins(botClient, message, replyMessages);
                    return;


                //Admin - 管理员命令
                case "GROUPINFO" when admin:
                    await AdminCmd.ResponseGroupInfo(botClient, message, replyMessages);
                    return;

                case "USERINFO" when admin:
                    return await AdminCmd.ResponseUserInfo(botClient, dbUser, message, args);

                case "NO" when admin:
                    return await AdminCmd.ResponseNo(botClient, dbUser, message, args);

                case "BAN" when admin:
                    return await AdminCmd.ResponseBan(botClient, dbUser, message, args);

                case "UNBAN" when admin:
                    return await AdminCmd.ResponseUnban(botClient, dbUser, message, args);

                case "QUERYBAN":
                    return await AdminCmd.ResponseQueryBan(botClient, dbUser, message, args);

                //case "YES" when admin:
                //    return await AdminCmd.ResponseYes(botClient, dbUser, message, payload);

                //Super
                case "SETUSERGROUP" when super:
                    return await SuperCmd.SetUserGroup(botClient, dbUser, message, args);

                case "RESTART" when super:
                    return await SuperCmd.ResponseRestart(botClient, dbUser, message);

                default:
                    return null;
            }

        }
    }
}
