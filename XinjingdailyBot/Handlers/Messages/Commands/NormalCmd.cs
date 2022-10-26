using XinjingdailyBot.Helpers;
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

            int totalPost = dbUser.PostCount - dbUser.ExpiredPostCount;

            StringBuilder sb = new();

            sb.AppendLine($"-- 基础信息 --");
            sb.AppendLine($"用户名: <code>{userNick}</code>");
            sb.AppendLine($"用户ID: <code>{dbUser.UserID}</code>");
            sb.AppendLine($"用户组: <code>{group}</code>");
            sb.AppendLine($"等级:  <code>{level}</code>");
            sb.AppendLine($"投稿数量: <code>{totalPost}</code>");
            sb.AppendLine($"通过率: <code>{(100.0 * dbUser.AcceptCount / totalPost).ToString("0.00")}%</code>");
            sb.AppendLine($"通过数量: <code>{dbUser.AcceptCount}</code>");
            sb.AppendLine($"拒绝数量: <code>{dbUser.RejetCount}</code>");
            sb.AppendLine($"审核数量: <code>{dbUser.ReviewCount}</code>");
            sb.AppendLine($"-- 用户排名 --");

            DateTime now = DateTime.Now;
            DateTime prev30Days = now.AddDays(-30).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);

            if (dbUser.AcceptCount > 10 && dbUser.GroupID == 1)
            {
                int activeUser = await DB.Queryable<Users>().Where(x => !x.IsBan && !x.IsBot && x.ModifyAt >= prev30Days).CountAsync();
                int acceptCountRank = await DB.Queryable<Users>().Where(x => !x.IsBan && !x.IsBot && x.GroupID == 1 && x.AcceptCount > dbUser.AcceptCount && x.ModifyAt >= prev30Days).CountAsync() + 1;

                double ratio = 1.0 * dbUser.AcceptCount / dbUser.PostCount;

                int acceptRatioRank = await DB.Queryable<Users>().Where(x => !x.IsBan && !x.IsBot && x.GroupID == 1 && x.AcceptCount > 10 && x.ModifyAt >= prev30Days)
                  .Select(y => new { Ratio = y.AcceptCount / y.PostCount }).Where(x => x.Ratio > ratio).CountAsync() + 1;

                sb.AppendLine($"通过数量: <code>{acceptCountRank}</code>");
                sb.AppendLine($"通过率: <code>{acceptRatioRank}</code>");
                sb.AppendLine($"活跃用户: <code>{activeUser}</code>");
            }
            else
            {
                if (dbUser.GroupID != 1)
                {
                    sb.AppendLine($"管理员不参与用户排名");
                    sb.AppendLine($"可以使用命令 /userrank 查看管理员排名");
                }
                else
                {
                    sb.AppendLine("稿件数量太少, 未进入排行榜");
                }
            }

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
            if (superCmd) { commands.Add("所有命令"); }
            if (adminCmd) { commands.Add("管理员命令"); }
            if (normalCmd) { commands.Add("普通命令"); }
            if (functions.Count == 0) { commands.Add("无"); }

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
