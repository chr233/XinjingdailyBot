using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Handlers.Messages.Commands;
using XinjingdailyBot.Helpers;
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
            CmdRecords record = new()
            {
                ChatID = message.Chat.Id,
                MessageID = message.MessageId,
                UserID = dbUser.UserID,
                Command = message.Text![1..],
                Handled = false,
                IsQuery = false,
                ExecuteAt = DateTime.Now,
            };

            bool needRecord = true;

            try
            {
                (needRecord, record.Handled, bool autoDelete) = await ExecCommand(botClient, dbUser, message);

                //定时删除命令消息
                if (autoDelete)
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
            catch (Exception ex)
            {
                record.Exception = $"{ex.GetType} {ex.Message}";
                record.Error = true;
                throw;
            }
            finally
            {
                if (needRecord)
                {
                    await DB.Insertable(record).ExecuteCommandAsync();
                }
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message">用户消息原文</param>
        /// <returns>needRecord,handled,autoDelete</returns>
        private static async Task<(bool, bool, bool)> ExecCommand(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            //切分命令参数
            string[] args = message.Text!.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
            if (!args.Any()) { return (false, false, false); }

            string cmd = args.First()[1..];
            args = args[1..];

            bool inGroup = message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup;

            //判断是不是艾特机器人的命令
            bool isAtBot = false;
            int index = cmd.IndexOf('@');
            if (inGroup && index != -1)
            {
                string botName = cmd[(index + 1)..];
                if (botName.Equals(BotName, StringComparison.OrdinalIgnoreCase))
                {
                    isAtBot = true;
                    cmd = cmd[..index];
                }
                else
                {
                    return (false, false, false);
                }
            }

            //检查权限
            bool super = dbUser.Right.HasFlag(UserRights.SuperCmd);
            bool admin = dbUser.Right.HasFlag(UserRights.AdminCmd) || super;
            bool normal = dbUser.Right.HasFlag(UserRights.NormalCmd) || admin;
            bool reviewPost = dbUser.Right.HasFlag(UserRights.ReviewPost);

            //是否自动删除消息
            bool autoDelete = true;
            //是否成功响应命令
            bool handled = true;
            switch (cmd.ToUpperInvariant())
            {
                //Common - 通用命令, 不鉴权
                case "HELP":
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

                case "QUERYBAN" when admin:
                    await AdminCmd.ResponseQueryBan(botClient, dbUser, message, args);
                    break;

                case "ECHO" when admin:
                    await AdminCmd.ResponseEcho(botClient, dbUser, message, args);
                    autoDelete = false;
                    break;

                case "SEARCHUSER" when admin:
                    await AdminCmd.ResponseSearchUser(botClient, dbUser, message, args);
                    break;

                case "SYSREPORT" when admin:
                    await AdminCmd.ResponseSystemReport(botClient, dbUser, message, args);
                    break;

                //Super - 超级管理员命令
                case "RESTART" when super:
                    await SuperCmd.ResponseRestart(botClient, message);
                    break;

                case "SETUSERGROUP" when super:
                    await SuperCmd.SetUserGroup(botClient, dbUser, message, args);
                    break;

                //Review - 审核命令
                case "NO" when reviewPost:
                    await ReviewCmd.ResponseNo(botClient, dbUser, message, args);
                    autoDelete = false;
                    break;

                case "EDIT" when reviewPost:
                    await ReviewCmd.ResponseEditPost(botClient, dbUser, message, args);
                    autoDelete = false;
                    break;

                default:
                    //仅在私聊,或者艾特机器人时提示未知命令
                    if (isAtBot || !inGroup)
                    {
                        await botClient.SendCommandReply("未知命令, 获取帮助 /help", message, false);
                    }
                    handled = false;
                    break;
            }

            //自动删除命令的时机
            //1.autoDelete = true
            //2.在群组中
            //3.成功执行命令
            return (true, handled, autoDelete && inGroup && handled);
        }
    }
}
