using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Messages.Commands
{
    internal static class NormalCmd
    {
        /// <summary>
        /// 测试机器人是否存活
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task ResponsePing(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendCommandReply("PONG!", message);
        }

        /// <summary>
        /// 设置是否匿名
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task ResponseAnymouse(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            bool anymouse = !dbUser.PreferAnymouse;
            dbUser.PreferAnymouse = anymouse;
            dbUser.ModifyAt = DateTime.Now;
            await DB.Updateable(dbUser).UpdateColumns(x => new { x.PreferAnymouse, x.ModifyAt }).ExecuteCommandAsync();

            string mode = anymouse ? "匿名投稿" : "保留来源";
            string text = $"后续投稿将默认使用【{mode}】";
            await botClient.SendCommandReply(text, message);
        }

        /// <summary>
        /// 设置稿件审核后是否通知
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task ResponseNotification(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            bool notificationg = !dbUser.Notification;
            dbUser.Notification = notificationg;
            dbUser.ModifyAt = DateTime.Now;
            await DB.Updateable(dbUser).UpdateColumns(x => new { x.Notification, x.ModifyAt }).ExecuteCommandAsync();

            string mode = notificationg ? "接收通知" : "静默模式";
            string text = $"稿件被审核或者过期时将会尝试通知用户\n当前通知设置: {mode}";
            await botClient.SendCommandReply(text, message);
        }


        /// <summary>
        /// 获取自己的信息
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task ResponseMyInfo(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            string userNick = TextHelper.EscapeHtml(dbUser.UserNick);
            string level = "Lv Err";
            if (ULevels.TryGetValue(dbUser.Level, out var l))
            {
                level = l.Name;
            }
            string group = "???";
            if (UGroups.TryGetValue(dbUser.GroupID, out var g))
            {
                group = g.Name;
            }

            StringBuilder sb = new();
            sb.AppendLine($"用户名: <code>{userNick}</code>");
            sb.AppendLine($"用户ID: <code>{dbUser.UserID}</code>");
            sb.AppendLine($"用户组: <code>{group}</code>");
            sb.AppendLine($"等级:  <code>{level}</code>");
            sb.AppendLine($"投稿数量: <code>{dbUser.PostCount - dbUser.ExpiredPostCount}</code>");
            sb.AppendLine($"通过数量: <code>{dbUser.AcceptCount}</code>");
            sb.AppendLine($"拒绝数量: <code>{dbUser.RejetCount}</code>");
            sb.AppendLine($"审核数量: <code>{dbUser.ReviewCount}</code>");

            await botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
        }


        /// <summary>
        /// 获取自己的权限
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task ResponseMyRight(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            var right = dbUser.Right;
            bool superCmd = right.HasFlag(UserRights.SuperCmd);
            bool adminCmd = right.HasFlag(UserRights.AdminCmd);
            bool normalCmd = right.HasFlag(UserRights.NormalCmd);
            bool sendPost = right.HasFlag(UserRights.SendPost);
            bool reviewPost = right.HasFlag(UserRights.ReviewPost);
            bool directPost = right.HasFlag(UserRights.DirectPost);

            string userNick = TextHelper.EscapeHtml(dbUser.UserNick);

            string group = "???";
            if (UGroups.TryGetValue(dbUser.GroupID, out var g))
            {
                group = g.Name;
            }

            List<string> functions = new();
            if (sendPost) { functions.Add("投递稿件"); }
            if (reviewPost) { functions.Add("审核稿件"); }
            if (directPost) { functions.Add("直接发布稿件"); }
            if (functions.Count == 0) { functions.Add("无"); }

            List<string> commands = new();
            if (superCmd) { functions.Add("所有命令"); }
            if (adminCmd) { functions.Add("管理员命令"); }
            if (normalCmd) { functions.Add("普通命令"); }
            if (functions.Count == 0) { functions.Add("无"); }

            StringBuilder sb = new();
            sb.AppendLine($"用户名: <code>{userNick}</code>");
            sb.AppendLine($"用户组: <code>{group}</code>");
            sb.AppendLine($"功能: <code>{string.Join(", ", functions)}</code>");
            sb.AppendLine($"命令: <code>{string.Join(", ", commands)}</code>");

            await botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 艾特群管理
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task ResponseCallAdmins(ITelegramBotClient botClient, Message message)
        {
            StringBuilder sb = new();

            if (message.Chat.Type != ChatType.Group && message.Chat.Type != ChatType.Supergroup)
            {
                sb.AppendLine("该命令仅在群组内有效");
            }
            else
            {
                ChatMember[] admins = await botClient.GetChatAdministratorsAsync(message.Chat.Id);

                foreach (var menber in admins)
                {
                    var admin = menber.User;
                    if (!(admin.IsBot || string.IsNullOrEmpty(admin.Username)))
                    {
                        sb.AppendLine($"@{admin.Username}");
                    }
                }
            }

            await botClient.SendCommandReply(sb.ToString(), message);
        }
    }
}
