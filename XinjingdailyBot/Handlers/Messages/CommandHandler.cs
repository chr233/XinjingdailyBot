using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Handlers.Messages.Commands;
using XinjingdailyBot.Models;
using XinjingdailyBot.Helpers;
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
            bool inGroup = message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup;

            bool handled = await ExecCommand(botClient, dbUser, message);

            //定时删除群组中的命令消息
            if (inGroup && handled)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    try
                    {
                        await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                    }
                    catch
                    {
                        Logger.Error($"删除消息 {message.MessageId} 失败");
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
        /// <returns></returns>
        private static async Task<bool> ExecCommand(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            string[] args = message.Text!.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);

            if (!args.Any()) { return false; }

            string cmd = args.First()[1..];
            args = args[1..];

            //判断是不是自己的命令
            bool isAtBot = false;
            int index = cmd.IndexOf('@');
            if (index != -1)
            {
                string botName = cmd[(index + 1)..];
                if (botName.Equals(BotName, StringComparison.OrdinalIgnoreCase))
                {
                    isAtBot = true;
                    cmd = cmd[..index];
                }
                else
                {
                    return false;
                }
            }

            //检查权限
            bool super = dbUser.Right.HasFlag(UserRights.SuperCmd);
            bool admin = dbUser.Right.HasFlag(UserRights.AdminCmd) || super;
            bool normal = dbUser.Right.HasFlag(UserRights.NormalCmd) || admin;
            bool reviewPost = dbUser.Right.HasFlag(UserRights.ReviewPost);

            //响应命令
            bool handled = true;
            switch (cmd.ToUpperInvariant())
            {
                //Common - 通用命令, 不鉴权
                case "HELP" when normal:
                    await CommonCmd.ResponseHelp(botClient, dbUser, message);
                    break;

                case "START":
                    await CommonCmd.ResponseStart(botClient, dbUser, message);
                    break;

                case "VERSION":
                    await CommonCmd.ResponseVersion(botClient, message);
                    break;

                case "MYBAN":
                    await CommonCmd.ResponseMyBan(botClient, dbUser, message);
                    break;

                //Normal - 普通命令
                case "PING" when normal:
                    await NormalCmd.ResponsePing(botClient, message);
                    break;

                case "ANONYMOUS" when normal:
                    await NormalCmd.ResponseAnymouse(botClient, dbUser, message);
                    break;

                case "NOTIFICATION" when normal:
                    await NormalCmd.ResponseNotification(botClient, dbUser, message);
                    break;

                case "MYINFO" when normal:
                    await NormalCmd.ResponseMyInfo(botClient, dbUser, message);
                    break;

                case "MYRIGHT" when normal:
                    await NormalCmd.ResponseMyRight(botClient, dbUser, message);
                    break;

                case "ADMIN" when normal:
                case "ADMINS" when normal:
                    await NormalCmd.ResponseCallAdmins(botClient, message);
                    break;

                //Admin - 管理员命令
                case "GROUPINFO" when admin:
                    await AdminCmd.ResponseGroupInfo(botClient, message);
                    break;

                case "USERINFO" when admin:
                    await AdminCmd.ResponseUserInfo(botClient, dbUser, message, args);
                    break;

                case "BAN" when admin:
                    await AdminCmd.ResponseBan(botClient, dbUser, message, args);
                    break;

                case "UNBAN" when admin:
                    await AdminCmd.ResponseUnban(botClient, dbUser, message, args);
                    break;

                case "QUERYBAN":
                    await AdminCmd.ResponseQueryBan(botClient, dbUser, message, args);
                    break;

                //Super
                case "SETUSERGROUP" when super:
                    await SuperCmd.SetUserGroup(botClient, dbUser, message, args);
                    break;

                case "RESTART" when super:
                    await SuperCmd.ResponseRestart(botClient, dbUser, message);
                    break;

                //Review - 审核命令
                case "NO" when reviewPost:
                    await ReviewCmd.ResponseNo(botClient, dbUser, message, args);
                    break;

                case "EDIT" when reviewPost:
                    await ReviewCmd.ResponseEditPost(botClient, dbUser, message, args);
                    break;

                default:
                    //仅在私聊,或者艾特机器人时提示未知命令
                    if (isAtBot || message.Chat.Type == ChatType.Private)
                    {
                        await botClient.SendCommandReply("未知命令, 获取帮助 /help", message);
                    }
                    else
                    {
                        handled = false;
                    }
                    break;
            }
            return handled;
        }
    }
}
