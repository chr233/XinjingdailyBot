using System.Diagnostics;
using System.Runtime.InteropServices;
using MySqlX.XDevAPI.Relational;
using SqlSugar;
using XinjingdailyBot.Helpers;
using static XinjingdailyBot.Utils;
using System.IO;
using System;

namespace XinjingdailyBot.Handlers.Messages.Commands
{
    internal static class AdminCmd
    {
        /// <summary>
        /// 被警告超过此值自动封禁
        /// </summary>
        private const int WarningLimit = 3;

        /// <summary>
        /// 获取群组信息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task ResponseGroupInfo(ITelegramBotClient botClient, Message message)
        {
            var chat = message.Chat;

            StringBuilder sb = new();

            if (chat.Type != ChatType.Group && chat.Type != ChatType.Supergroup)
            {
                sb.AppendLine("该命令仅限群组内使用");
            }
            else
            {
                sb.AppendLine($"群组名: <code>{chat.Title ?? "无"}</code>");

                if (string.IsNullOrEmpty(chat.Username))
                {
                    sb.AppendLine("群组类型: <code>私聊</code>");
                    sb.AppendLine($"群组ID: <code>{chat.Id}</code>");
                }
                else
                {
                    sb.AppendLine("群组类型: <code>公开</code>");
                    sb.AppendLine($"群组链接: <code>@{chat.Username ?? "无"}</code>");
                }
            }
            await botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task ResponseUserInfo(ITelegramBotClient botClient, Message message, string[] args)
        {
            StringBuilder sb = new();

            var targetUser = await FetchUserHelper.FetchTargetUser(message);

            if (targetUser == null)
            {
                if (args.Any())
                {
                    targetUser = await FetchUserHelper.FetchDbUserByUserNameOrUserID(args.First());
                }
            }

            if (targetUser == null)
            {
                sb.AppendLine("找不到指定用户, 你可以手动指定用户名/用户ID");
            }
            else
            {
                string userNick = TextHelper.EscapeHtml(targetUser.UserNick);
                string level = "Lv Err";
                if (ULevels.TryGetValue(targetUser.Level, out var l))
                {
                    level = l.Name;
                }
                string group = "???";
                if (UGroups.TryGetValue(targetUser.GroupID, out var g))
                {
                    group = g.Name;
                }
                string status = targetUser.IsBan ? "封禁中" : "正常";

                int totalPost = targetUser.PostCount - targetUser.ExpiredPostCount;

                sb.AppendLine($"用户名: <code>{userNick}</code>");
                sb.AppendLine($"用户ID: <code>{targetUser.UserID}</code>");
                sb.AppendLine($"用户组: <code>{group}</code>");
                sb.AppendLine($"状态: <code>{status}</code>");
                sb.AppendLine($"等级:  <code>{level}</code>");
                sb.AppendLine($"投稿数量: <code>{totalPost}</code>");
                sb.AppendLine($"通过率: <code>{100.0 * targetUser.AcceptCount / totalPost}%</code>");
                sb.AppendLine($"通过数量: <code>{targetUser.AcceptCount}</code>");
                sb.AppendLine($"拒绝数量: <code>{targetUser.RejetCount}</code>");
                sb.AppendLine($"审核数量: <code>{targetUser.ReviewCount}</code>");
            }

            await botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 封禁用户
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task ResponseBan(ITelegramBotClient botClient, Users dbUser, Message message, string[] args)
        {
            async Task<string> exec()
            {
                var targetUser = await FetchUserHelper.FetchTargetUser(message);

                if (targetUser == null)
                {
                    if (args.Any())
                    {
                        targetUser = await FetchUserHelper.FetchDbUserByUserNameOrUserID(args.First());
                        args = args[1..];
                    }
                }

                if (targetUser == null)
                {
                    return "找不到指定用户";
                }

                if (targetUser.Id == dbUser.Id)
                {
                    return "无法对自己进行操作";
                }

                if (targetUser.GroupID >= dbUser.GroupID)
                {
                    return "无法对同级管理员进行此操作";
                }

                string reason = string.Join(' ', args);
                if (string.IsNullOrEmpty(reason))
                {
                    return "请指定封禁理由";
                }

                if (targetUser.IsBan)
                {
                    return "当前用户已经封禁, 请不要重复操作";
                }
                else
                {
                    targetUser.IsBan = true;
                    targetUser.ModifyAt = DateTime.Now;
                    await DB.Updateable(targetUser).UpdateColumns(x => new { x.IsBan, x.ModifyAt }).ExecuteCommandAsync();

                    var record = new BanRecords()
                    {
                        UserID = targetUser.UserID,
                        OperatorUID = dbUser.UserID,
                        Type = BanType.Ban,
                        BanTime = DateTime.Now,
                        Reason = reason,
                    };

                    await DB.Insertable(record).ExecuteCommandAsync();

                    try
                    {
                        if (targetUser.PrivateChatID > 0)
                        {
                            string msg = string.Format(Langs.BanedUserTips, "管理员", reason);
                            await botClient.SendTextMessageAsync(targetUser.PrivateChatID, msg);
                            await botClient.SendTextMessageAsync(targetUser.PrivateChatID, Langs.QueryBanDescribe);
                        }
                        else
                        {
                            Logger.Info("用户未私聊过机器人, 无法发送消息");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"发送私聊消息失败, {ex.Message}");
                    }

                    StringBuilder sb = new();
                    sb.AppendLine($"成功封禁 {TextHelper.HtmlUserLink(targetUser)}");
                    sb.AppendLine($"操作员 {TextHelper.HtmlUserLink(dbUser)}");
                    sb.AppendLine($"封禁理由 <code>{reason}</code>");
                    return sb.ToString();
                }
            }

            string text = await exec();
            await botClient.SendCommandReply(text, message, false, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 解封用户
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task ResponseUnban(ITelegramBotClient botClient, Users dbUser, Message message, string[] args)
        {
            async Task<string> exec()
            {
                var targetUser = await FetchUserHelper.FetchTargetUser(message);

                if (targetUser == null)
                {
                    if (args.Any())
                    {
                        targetUser = await FetchUserHelper.FetchDbUserByUserNameOrUserID(args.First());
                        args = args[1..];
                    }
                }

                if (targetUser == null)
                {
                    return "找不到指定用户";
                }

                if (targetUser.Id == dbUser.Id)
                {
                    return "无法对自己进行操作";
                }

                if (targetUser.GroupID >= dbUser.GroupID)
                {
                    return "无法对同级管理员进行此操作";
                }

                string reason = string.Join(' ', args);
                if (string.IsNullOrEmpty(reason))
                {
                    return "请指定解封理由";
                }

                if (!targetUser.IsBan)
                {
                    return "当前用户未被封禁或者已经解封, 请不要重复操作";
                }
                else
                {
                    targetUser.IsBan = false;
                    targetUser.ModifyAt = DateTime.Now;
                    await DB.Updateable(targetUser).UpdateColumns(x => new { x.IsBan, x.ModifyAt }).ExecuteCommandAsync();

                    var record = new BanRecords()
                    {
                        UserID = targetUser.UserID,
                        OperatorUID = dbUser.UserID,
                        Type = BanType.UnBan,
                        BanTime = DateTime.Now,
                        Reason = reason,
                    };

                    await DB.Insertable(record).ExecuteCommandAsync();

                    try
                    {
                        if (targetUser.PrivateChatID > 0)
                        {
                            string msg = string.Format(Langs.UnbanedUserTips, "管理员", reason);
                            await botClient.SendTextMessageAsync(targetUser.PrivateChatID, msg);
                            await botClient.SendTextMessageAsync(targetUser.PrivateChatID, Langs.QueryBanDescribe);
                        }
                        else
                        {
                            Logger.Info("用户未私聊过机器人, 无法发送消息");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"发送私聊消息失败, {ex.Message}");
                    }

                    StringBuilder sb = new();
                    sb.AppendLine($"成功解封 {TextHelper.HtmlUserLink(targetUser)}");
                    sb.AppendLine($"操作员 {TextHelper.HtmlUserLink(dbUser)}");
                    sb.AppendLine($"解封理由 <code>{reason}</code>");
                    return sb.ToString();
                }
            }

            string text = await exec();
            await botClient.SendCommandReply(text, message, false, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 警告用户
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task ResponseWarning(ITelegramBotClient botClient, Users dbUser, Message message, string[] args)
        {
            async Task<string> exec()
            {
                var targetUser = await FetchUserHelper.FetchTargetUser(message);

                if (targetUser == null)
                {
                    if (args.Any())
                    {
                        targetUser = await FetchUserHelper.FetchDbUserByUserNameOrUserID(args.First());
                        args = args[1..];
                    }
                }

                if (targetUser == null)
                {
                    return "找不到指定用户";
                }

                if (targetUser.Id == dbUser.Id)
                {
                    return "无法对自己进行操作";
                }

                if (targetUser.GroupID >= dbUser.GroupID)
                {
                    return "无法对同级管理员进行此操作";
                }

                string reason = string.Join(' ', args);
                if (string.IsNullOrEmpty(reason))
                {
                    return "请指定警告理由";
                }

                if (targetUser.IsBan)
                {
                    return "当前用户已被封禁, 无法再发送警告";
                }
                else
                {
                    //获取最近一条解封记录
                    var lastUnbaned = await DB.Queryable<BanRecords>().Where(x => x.UserID == targetUser.UserID && (x.Type == BanType.UnBan || x.Type == BanType.Ban))
                        .OrderByDescending(x => x.BanTime).FirstAsync();

                    int warnCount;
                    if (lastUnbaned == null)
                    {
                        warnCount = await DB.Queryable<BanRecords>().Where(x => x.UserID == targetUser.UserID && x.Type == BanType.Warning).CountAsync();
                    }
                    else
                    {
                        warnCount = await DB.Queryable<BanRecords>().Where(x => x.UserID == targetUser.UserID && x.Type == BanType.Warning && x.BanTime >= lastUnbaned.BanTime).CountAsync();
                    }

                    var record = new BanRecords()
                    {
                        UserID = targetUser.UserID,
                        OperatorUID = dbUser.UserID,
                        Type = BanType.Warning,
                        BanTime = DateTime.Now,
                        Reason = reason,
                    };

                    await DB.Insertable(record).ExecuteCommandAsync();

                    warnCount++;

                    StringBuilder sb = new();
                    sb.AppendLine($"成功警告 {TextHelper.HtmlUserLink(targetUser)}");
                    sb.AppendLine($"操作员 {TextHelper.HtmlUserLink(dbUser)}");
                    sb.AppendLine($"警告理由 <code>{reason}</code>");
                    sb.AppendLine($"累计警告 <code>{warnCount}</code> / <code>{WarningLimit}</code> 次");

                    if (warnCount >= WarningLimit)
                    {
                        record = new BanRecords()
                        {
                            UserID = targetUser.UserID,
                            OperatorUID = 0,
                            Type = BanType.Ban,
                            BanTime = DateTime.Now,
                            Reason = "受到警告过多, 自动封禁",
                        };

                        await DB.Insertable(record).ExecuteCommandAsync();

                        targetUser.IsBan = true;
                        targetUser.ModifyAt = DateTime.Now;
                        await DB.Updateable(targetUser).UpdateColumns(x => new { x.IsBan, x.ModifyAt }).ExecuteCommandAsync();

                        sb.AppendLine($"受到警告过多, 系统自动封禁该用户");
                    }

                    try
                    {
                        if (targetUser.PrivateChatID > 0)
                        {
                            string msg = string.Format(Langs.WarnUserTips, reason, warnCount, WarningLimit);
                            await botClient.SendTextMessageAsync(targetUser.PrivateChatID, msg, ParseMode.Html);

                            if (warnCount >= WarningLimit)
                            {
                                msg = string.Format(Langs.BanedUserTips, "系统", "自动封禁");
                                await botClient.SendTextMessageAsync(targetUser.PrivateChatID, msg);
                            }

                            await botClient.SendTextMessageAsync(targetUser.PrivateChatID, Langs.QueryBanDescribe);
                        }
                        else
                        {
                            Logger.Info("用户未私聊过机器人, 无法发送消息");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"发送私聊消息失败, {ex.Message}");
                    }

                    return sb.ToString();
                }
            }

            string text = await exec();
            await botClient.SendCommandReply(text, message, false, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 查询封禁记录
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task ResponseQueryBan(ITelegramBotClient botClient, Message message, string[] args)
        {
            StringBuilder sb = new();

            var targetUser = await FetchUserHelper.FetchTargetUser(message);

            if (targetUser == null)
            {
                if (args.Any())
                {
                    targetUser = await FetchUserHelper.FetchDbUserByUserNameOrUserID(args.First());
                    args = args[1..];
                }
            }

            if (targetUser == null)
            {
                sb.AppendLine("找不到指定用户");
            }
            else
            {
                var records = await DB.Queryable<BanRecords>().Where(x => x.UserID == targetUser.UserID).ToListAsync();

                string status = targetUser.IsBan ? "已封禁" : "正常";
                sb.AppendLine($"用户名: <code>{targetUser.UserNick}</code>");
                sb.AppendLine($"用户ID: <code>{targetUser.UserID}</code>");
                sb.AppendLine($"状态: <code>{status}</code>");
                sb.AppendLine();

                if (records == null)
                {
                    sb.AppendLine("查询封禁/解封记录出错");
                }
                else if (!records.Any())
                {
                    sb.AppendLine("尚未查到封禁/解封记录");
                }
                else
                {
                    var operators = records.Select(x => x.OperatorUID).Distinct();

                    var users = await DB.Queryable<Users>().Where(x => operators.Contains(x.UserID)).Distinct().ToListAsync();

                    foreach (var record in records)
                    {
                        string date = record.BanTime.ToString("d");
                        string operate = record.Type switch
                        {
                            BanType.UnBan => "解封",
                            BanType.Ban => "封禁",
                            BanType.Warning => "警告",
                            _ => "未知",
                        };

                        string admin;
                        if (record.OperatorUID == 0)
                        {
                            admin = "系统";
                        }
                        else
                        {
                            var user = users.Find(x => x.UserID == record.OperatorUID);
                            admin = user != null ? user.UserNick : record.OperatorUID.ToString();
                        }

                        sb.AppendLine($"在 <code>{date}</code> 因为 <code>{record.Reason}</code> 被 <code>{admin}</code> {operate}");
                    }
                }
            }

            await botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 回复用户
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task ResponseEcho(ITelegramBotClient botClient, Users dbUser, Message message, string[] args)
        {
            bool autoDelete = true;
            async Task<string> exec()
            {
                var targetUser = await FetchUserHelper.FetchTargetUser(message);

                if (targetUser == null)
                {
                    if (args.Any())
                    {
                        targetUser = await FetchUserHelper.FetchDbUserByUserNameOrUserID(args.First());
                        args = args[1..];
                    }
                }

                if (targetUser == null)
                {
                    return "找不到指定用户";
                }

                if (targetUser.UserID == dbUser.UserID)
                {
                    return "为什么有人想要自己回复自己?";
                }

                if (targetUser.PrivateChatID <= 0)
                {
                    return "该用户尚未私聊过机器人, 无法发送消息";
                }

                string msg = string.Join(' ', args).Trim();

                if (string.IsNullOrEmpty(msg))
                {
                    return "请输入回复内容";
                }

                autoDelete = false;
                try
                {
                    msg = TextHelper.EscapeHtml(msg);
                    await botClient.SendTextMessageAsync(targetUser.PrivateChatID, $"来自管理员的消息:\n<code>{msg}</code>", ParseMode.Html);
                    return "消息发送成功";
                }
                catch (Exception ex)
                {
                    return $"消息发送失败 {ex.Message}";
                }
            }

            string text = await exec();
            await botClient.SendCommandReply(text, message, autoDelete);
        }

        /// <summary>
        /// 搜索用户
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task ResponseSearchUser(ITelegramBotClient botClient, Users dbUser, Message message, string[] args)
        {
            if (!args.Any())
            {
                await botClient.SendCommandReply("请指定 用户昵称/用户名/用户ID 作为查询参数", message, true);
                return;
            }

            string query = args.First();

            int page;
            if (args.Length >= 2)
            {
                if (!int.TryParse(args[1], out page))
                {
                    page = 1;
                }
            }
            else
            {
                page = 1;
            }

            (string text, var keyboard) = await FetchUserHelper.QueryUserList(dbUser, query, page);

            await botClient.SendCommandReply(text, message, false, ParseMode.Html, keyboard);
        }

        /// <summary>
        /// 生成投稿统计信息
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task ResponsePostReport(ITelegramBotClient botClient, Message message)
        {
            DateTime now = DateTime.Now;
            DateTime prev1Day = now.AddDays(-1).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);
            DateTime prev7Days = now.AddDays(-7).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);
            DateTime prev30Days = now.AddDays(-30).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);
            DateTime monthStart = now.AddDays(1 - now.Day).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);
            DateTime yearStart = now.AddMonths(1 - now.Month).AddDays(1 - now.Day).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);

            StringBuilder sb = new();

            int todayPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= prev1Day && x.Status > PostStatus.Cancel).CountAsync();
            int todayAcceptPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= prev1Day && x.Status == PostStatus.Accepted).CountAsync();
            int todayRejectPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= prev1Day && x.Status == PostStatus.Rejected).CountAsync();
            int todayExpiredPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= prev1Day && x.Status < 0).CountAsync();

            sb.AppendLine("-- 24小时投稿统计 --");
            sb.AppendLine($"接受/拒绝: <code>{todayAcceptPost}</code> / <code>{todayRejectPost}</code>");
            sb.AppendLine($"通过率: <code>{(todayPost > 0 ? (100 * todayAcceptPost / todayPost).ToString("f2") : "--")}%</code>");
            sb.AppendLine($"过期投稿: <code>{todayExpiredPost}</code>");
            sb.AppendLine($"累计投稿: <code>{todayPost + todayExpiredPost}</code>");

            int weekPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= prev7Days && x.Status > PostStatus.Cancel).CountAsync();
            int weekAcceptPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= prev7Days && x.Status == PostStatus.Accepted).CountAsync();
            int weekRejectPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= prev7Days && x.Status == PostStatus.Rejected).CountAsync();
            int weekExpiredPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= prev7Days && x.Status < 0).CountAsync();

            sb.AppendLine();
            sb.AppendLine("-- 7日投稿统计 --");
            sb.AppendLine($"接受/拒绝: <code>{weekAcceptPost}</code> / <code>{weekRejectPost}</code>");
            sb.AppendLine($"通过率: <code>{(weekPost > 0 ? (100 * weekAcceptPost / weekPost).ToString("f2") : "--")}%</code>");
            sb.AppendLine($"过期投稿: <code>{weekExpiredPost}</code>");
            sb.AppendLine($"累计投稿: <code>{weekPost + weekExpiredPost}</code>");

            int monthPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= monthStart && x.Status > PostStatus.Cancel).CountAsync();
            int monthAcceptPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= monthStart && x.Status == PostStatus.Accepted).CountAsync();
            int monthRejectPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= monthStart && x.Status == PostStatus.Rejected).CountAsync();
            int monthExpiredPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= monthStart && x.Status < 0).CountAsync();

            sb.AppendLine();
            sb.AppendLine($"-- {monthStart.ToString("MM")}月投稿统计 --");
            sb.AppendLine($"接受/拒绝: <code>{monthAcceptPost}</code> / <code>{monthRejectPost}</code>");
            sb.AppendLine($"通过率: <code>{(monthPost > 0 ? (100 * monthAcceptPost / monthPost).ToString("f2") : "--")}%</code>");
            sb.AppendLine($"过期投稿: <code>{monthExpiredPost}</code>");
            sb.AppendLine($"累计投稿: <code>{monthPost}</code>");

            int yearPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= yearStart && x.Status > PostStatus.Cancel).CountAsync();
            int yearAcceptPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= yearStart && x.Status == PostStatus.Accepted).CountAsync();
            int yearRejectPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= yearStart && x.Status == PostStatus.Rejected).CountAsync();
            int yearExpiredPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= yearStart && x.Status < 0).CountAsync();

            sb.AppendLine();
            sb.AppendLine($"-- {yearStart.ToString("yyyy")}年投稿统计 --");
            sb.AppendLine($"接受/拒绝: <code>{yearAcceptPost}</code> / <code>{yearRejectPost}</code>");
            sb.AppendLine($"通过率: <code>{(yearPost > 0 ? (100 * yearAcceptPost / yearPost).ToString("f2") : "--")}%</code>");
            sb.AppendLine($"过期投稿: <code>{yearExpiredPost}</code>");
            sb.AppendLine($"累计投稿: <code>{yearPost}</code>");

            int totalPost = await DB.Queryable<Posts>().Where(x => x.Status > PostStatus.Cancel).CountAsync();
            int totalAcceptPost = await DB.Queryable<Posts>().Where(x => x.Status == PostStatus.Accepted).CountAsync();
            int totalRejectPost = await DB.Queryable<Posts>().Where(x => x.Status == PostStatus.Rejected).CountAsync();
            int totalExpiredPost = await DB.Queryable<Posts>().Where(x => x.Status < 0).CountAsync();
            int totalAttachment = await DB.Queryable<Attachments>().CountAsync();

            sb.AppendLine();
            sb.AppendLine("-- 历史投稿统计 --");
            sb.AppendLine($"接受/拒绝: <code>{totalAcceptPost}</code> / <code>{totalRejectPost}</code>");
            sb.AppendLine($"通过率: <code>{(totalPost > 0 ? (100 * totalAcceptPost / totalPost).ToString("f2") : "--")}%</code>");
            sb.AppendLine($"过期投稿: <code>{totalExpiredPost}</code>");
            sb.AppendLine($"累计投稿: <code>{totalPost}</code>");
            sb.AppendLine($"附件总数: <code>{totalAttachment}</code>");

            int totalUser = await DB.Queryable<Users>().CountAsync();
            int banedUser = await DB.Queryable<Users>().Where(x => x.IsBan).CountAsync();
            int weekActiveUser = await DB.Queryable<Users>().Where(x => x.ModifyAt >= prev7Days).CountAsync();
            int MonthActiveUser = await DB.Queryable<Users>().Where(x => x.ModifyAt >= prev30Days).CountAsync();
            int postedUser = await DB.Queryable<Users>().Where(x => x.PostCount > 0).CountAsync();

            sb.AppendLine();
            sb.AppendLine("-- 用户统计 --");
            sb.AppendLine($"封禁用户: <code>{banedUser}</code>");
            sb.AppendLine($"周活用户: <code>{weekActiveUser}</code>");
            sb.AppendLine($"月活用户: <code>{MonthActiveUser}</code>");
            sb.AppendLine($"投稿用户: <code>{postedUser}</code>");
            sb.AppendLine($"累计用户: <code>{totalUser}</code>");

            await botClient.SendCommandReply(sb.ToString(), message, true, ParseMode.Html);
        }

        /// <summary>
        /// 生成系统报表
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task ResponseSystemReport(ITelegramBotClient botClient, Message message)
        {
            StringBuilder sb = new();

            var drivers = DriveInfo.GetDrives();

            sb.AppendLine("-- 硬盘信息 --");
            foreach (var d in drivers)
            {
                if (d.IsReady)
                {
                    double freeSpacePerc = (d.AvailableFreeSpace / (float)d.TotalSize) * 100;

                    // Ouput drive information
                    sb.AppendLine($"Drive: {d.Name} ({d.DriveFormat}, {d.DriveType})");

                    sb.AppendLine($"Percentage free space: {freeSpacePerc:0.00}%.")                        ;

                }
            }


            var proc = Process.GetCurrentProcess();
            double mem = proc.WorkingSet64 / 1024.0 / 1024.0;
            TimeSpan cpu = proc.TotalProcessorTime;

            sb.AppendLine("-- 运行环境 --");
            sb.AppendLine($"占用内存: <code>{mem.ToString("f2")}</code> MB");
            sb.AppendLine($"运行时间: <code>{cpu.TotalMinutes.ToString("0.00")}</code> 天");
            sb.AppendLine($".NET版本: <code>.Net {Environment.Version} {RuntimeInformation.OSArchitecture}</code>");
            sb.AppendLine($"系统信息: <code>{RuntimeInformation.FrameworkDescription}</code>");

            await botClient.SendCommandReply(sb.ToString(), message, true, ParseMode.Html);
        }

        /// <summary>
        /// 创建审核群的邀请链接
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task ResponseInviteToReviewGroup(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            if (ReviewGroup.Id == -1)
            {
                await botClient.SendCommandReply("尚未设置审核群, 无法完成此操作", message);
                return;
            }

            if (message.Chat.Type != ChatType.Private)
            {
                await botClient.SendCommandReply("该命令仅限私聊使用", message);
                return;
            }

            try
            {
                var inviteLink = await botClient.CreateChatInviteLinkAsync(ReviewGroup.Id, $"{dbUser} 创建的邀请链接", DateTime.Now.AddHours(1), 1, false);

                StringBuilder sb = new();

                sb.AppendLine($"创建 {ReviewGroup.Title} 的邀请链接成功, 一小时内有效, 仅限1人使用");
                sb.AppendLine($"<a href=\"{inviteLink.InviteLink}\">{TextHelper.EscapeHtml(inviteLink.Name ?? inviteLink.InviteLink)}</a>");

                Logger.Debug(sb.ToString());

                await botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
            }
            catch
            {
                await botClient.SendCommandReply("创建邀请链接失败, 可能未给予机器人邀请权限", message);
                throw;
            }
        }

        /// <summary>
        /// 查看用户排行榜
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task ResponseUserRank(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            DateTime now = DateTime.Now;
            DateTime prev30Days = now.AddDays(-30).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);

            const int topCount = 8;
            const int miniumPost = 10;

            StringBuilder sb = new();

            sb.AppendLine($"-- 用户投稿数量排名 --");
            var userAcceptCountRank = await DB.Queryable<Users>().Where(x => !x.IsBan && !x.IsBot && x.GroupID == 1 && x.AcceptCount > miniumPost && x.ModifyAt >= prev30Days)
                .OrderByDescending(x => x.AcceptCount).Take(topCount).ToListAsync();
            if (userAcceptCountRank?.Count > 0)
            {
                int count = 1;
                foreach (var user in userAcceptCountRank)
                {
                    sb.AppendLine($"{count++}. {(!user.PreferAnymouse ? user.UserNick : "匿名用户")} {user.AcceptCount}");
                }
            }
            else
            {
                sb.AppendLine("暂无数据");
            }

            sb.AppendLine($"-- 用户投稿通过率排名 --");
            var userAcceptRatioRank = await DB.Queryable<Users>().Where(x => !x.IsBan && !x.IsBot && x.GroupID == 1 && x.AcceptCount > miniumPost && x.ModifyAt >= prev30Days)
                .Select(y => new { User = y, Ratio = 100.0 * y.AcceptCount / y.PostCount }).OrderByDescending(x => x.Ratio).Take(topCount).ToListAsync();
            if (userAcceptRatioRank?.Count > 0)
            {
                int count = 1;
                foreach (var data in userAcceptRatioRank)
                {
                    var user = data.User;
                    sb.AppendLine($"{count++}. {(!user.PreferAnymouse ? user.UserNick : "匿名用户")} {user.AcceptCount} / {user.PostCount} {data.Ratio.ToString("0.00")}%");
                }
            }
            else
            {
                sb.AppendLine("暂无数据");
            }

            sb.AppendLine($"-- 管理员投稿数量排名 --");
            var adminAcceptCountRank = await DB.Queryable<Users>().Where(x => !x.IsBan && !x.IsBot && x.GroupID > 1 && x.AcceptCount > miniumPost && x.ModifyAt >= prev30Days)
                .OrderByDescending(x => x.AcceptCount).Take(topCount).ToListAsync();
            if (adminAcceptCountRank?.Count > 0)
            {
                int count = 1;
                foreach (var user in adminAcceptCountRank)
                {
                    sb.AppendLine($"{count++}. {(!user.PreferAnymouse ? user.UserNick : "匿名管理员")} {user.AcceptCount}");
                }
            }
            else
            {
                sb.AppendLine("暂无数据");
            }

            sb.AppendLine($"-- 管理员审核数量排名 --");
            var adminReviewCountRank = await DB.Queryable<Users>().Where(x => !x.IsBan && !x.IsBot && x.GroupID > 1 && x.ReviewCount > miniumPost && x.ModifyAt >= prev30Days)
                .OrderByDescending(x => x.ReviewCount).Take(topCount).ToListAsync();
            if (adminReviewCountRank?.Count > 0)
            {
                int count = 1;
                foreach (var user in adminReviewCountRank)
                {
                    sb.AppendLine($"{count++}. {(!user.PreferAnymouse ? user.UserNick : "匿名管理员")} {user.ReviewCount}");
                }
            }
            else
            {
                sb.AppendLine("暂无数据");
            }

            await botClient.SendCommandReply(sb.ToString(), message, false, ParseMode.Html);
        }
    }
}
