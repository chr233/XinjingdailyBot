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
        private static Dictionary<string, string> CommandList { get; } = new()
        {
            { "anymouse", "设置投稿是否默认匿名" },
            { "notification", "设置是否开启投稿通知" },
            { "A", "" },
            { "admin", "呼叫群管理" },
            { "B", "" },
            { "myinfo", "查询投稿数量" },
            { "myright", "查询权限信息" },
            { "C", "" },
            { "TODO", "" },
        };

        /// <summary>
        /// 显示命令帮助
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="admin"></param>
        /// <param name="super"></param>
        /// <returns></returns>
        internal static string ResponseHelp(Users dbUser)
        {
            StringBuilder sb = new();

            foreach (var cmd in CommandList)
            {
                if (!string.IsNullOrEmpty(cmd.Value))
                {
                    sb.AppendLine($"/{cmd.Key}  {cmd.Value}");
                }
                else
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        internal static string ResponseVersion()
        {
            return $"机器人版本: {MyVersion}";
        }

        /// <summary>
        /// 设置是否匿名
        /// </summary>
        /// <param name="dbUser"></param>
        /// <returns></returns>
        internal static async Task<string> ResponseAnymouse(Users dbUser)
        {
            bool anymouse = !dbUser.PreferAnymouse;
            dbUser.PreferAnymouse = anymouse;
            dbUser.ModifyAt = DateTime.Now;
            await DB.Updateable(dbUser).UpdateColumns(x => new { x.PreferAnymouse, x.ModifyAt }).ExecuteCommandAsync();

            string mode = anymouse ? "匿名投稿" : "保留来源";
            return $"后续投稿将默认使用【{mode}】";
        }

        /// <summary>
        /// 设置是否匿名
        /// </summary>
        /// <param name="dbUser"></param>
        /// <returns></returns>
        internal static async Task<string> ResponseNotification(Users dbUser)
        {
            bool notificationg = !dbUser.Notification;
            dbUser.Notification = notificationg;
            dbUser.ModifyAt = DateTime.Now;
            await DB.Updateable(dbUser).UpdateColumns(x => new { x.Notification, x.ModifyAt }).ExecuteCommandAsync();

            string mode = notificationg ? "发送通知" : "静默模式";
            return $"投稿通过或者拒绝后将【{mode}】";
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <returns></returns>
        internal static string ResponseMyInfo(Users dbUser)
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
            sb.AppendLine($"用户组: <code>{group}</code>");
            sb.AppendLine($"等级:  <code>{level}</code>");
            sb.AppendLine($"投稿数量: <code>{dbUser.PostCount}</code>");
            sb.AppendLine($"通过数量: <code>{dbUser.AcceptCount}</code>");
            sb.AppendLine($"拒绝数量: <code>{dbUser.RejetCount}</code>");
            sb.AppendLine($"审核数量: <code>{dbUser.ReviewCount}</code>");

            return sb.ToString();
        }

        /// <summary>
        /// 获取用户权限
        /// </summary>
        /// <param name="dbUser"></param>
        /// <returns></returns>
        internal static string ResponseMyRight(Users dbUser)
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

            return sb.ToString();
        }

        /// <summary>
        /// 艾特群管理
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task<string> ResponseCallAdmins(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            if (message.Chat.Type != ChatType.Group && message.Chat.Type != ChatType.Supergroup)
            {
                return "该命令仅在群组内有效";
            }

            ChatMember[] admins = await botClient.GetChatAdministratorsAsync(message.Chat.Id);

            var adminsStr = admins.Where(x => !(x.User.IsBot || string.IsNullOrEmpty(x.User.Username))).Select(x => $"@{x.User.Username}");

            return string.Join('\n', adminsStr);
        }
    }
}
