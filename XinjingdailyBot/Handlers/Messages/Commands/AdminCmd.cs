using System.Text;
using SqlSugar;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Messages.Commands
{
    internal static class AdminCmd
    {
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
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task ResponseUserInfo(ITelegramBotClient botClient, Users dbUser, Message message, string[] args)
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

                sb.AppendLine($"用户名: <code>{userNick}</code>");
                sb.AppendLine($"用户ID: <code>{targetUser.UserID}</code>");
                sb.AppendLine($"用户组: <code>{group}</code>");
                sb.AppendLine($"等级:  <code>{level}</code>");
                sb.AppendLine($"投稿数量: <code>{targetUser.PostCount}</code>");
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

                string reason = args.Any() ? string.Join(' ', args) : "【未指定理由】";

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
                        IsBan = true,
                        BanTime = DateTime.Now,
                        Reason = reason,
                    };

                    await DB.Insertable(record).ExecuteCommandAsync();

                    return $"成功封禁该用户!\n理由: <code>{reason}</code>";
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

                string reason = args.Any() ? string.Join(' ', args) : "【未指定理由】";

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
                        IsBan = false,
                        BanTime = DateTime.Now,
                        Reason = reason,
                    };

                    await DB.Insertable(record).ExecuteCommandAsync();

                    return $"成功解封该用户!\n理由: <code>{reason}</code>";
                }
            }

            string text = await exec();
            await botClient.SendCommandReply(text, message, false, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 查询封禁记录
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task ResponseQueryBan(ITelegramBotClient botClient, Users dbUser, Message message, string[] args)
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
                    foreach (var record in records)
                    {
                        string date = record.BanTime.ToString("d");
                        string operate = record.IsBan ? "受到封禁" : "被解封";
                        sb.AppendLine($"在 <code>{date}</code> 因为 <code>{record.Reason}</code> {operate}");
                    }
                }
            }

            await botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 回复用户
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
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
        /// 生成系统报表
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task ResponseSystemReport(ITelegramBotClient botClient, Users dbUser, Message message, string[] args)
        {
            StringBuilder sb = new();

            var totalPost = await DB.Queryable<Posts>().CountAsync();
            var totalAcceptPost = await DB.Queryable<Posts>().Where(x => x.Status == PostStatus.Accepted).CountAsync();
            var totalRejectPost = await DB.Queryable<Posts>().Where(x => x.Status == PostStatus.Rejected).CountAsync();

            sb.AppendLine("-- 累计投稿 --");
            sb.AppendLine($"接受/拒绝: <code>{totalAcceptPost}</code>/<code>{totalRejectPost}</code>");
            if (totalPost > 0)
            {
                sb.AppendLine($"通过率: <code>{(100 * totalAcceptPost / totalPost).ToString("f2")}%</code>");
            }
            else
            {
                sb.AppendLine("通过率: <code> --%</code>");
            }
            sb.AppendLine($"总计投稿: <code>{totalPost}</code>");

            DateTime now = DateTime.Now;
            DateTime monthStart = DateTime.Now.AddDays(1 - now.Day).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);

            var monthPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= monthStart).CountAsync();
            var monthAcceptPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= monthStart && x.Status == PostStatus.Accepted).CountAsync();
            var monthRejectPost = await DB.Queryable<Posts>().Where(x => x.CreateAt >= monthStart && x.Status == PostStatus.Rejected).CountAsync();

            sb.AppendLine();
            sb.AppendLine($"-- 自 {monthStart.ToString("yyyy-mm-dd")} 起 --");
            sb.AppendLine($"接受/拒绝: <code>{monthAcceptPost}</code>/<code>{monthRejectPost}</code>");
            if (monthPost > 0)
            {
                sb.AppendLine($"通过率: <code>{(100 * monthAcceptPost / monthPost).ToString("f2")}%</code>");
            }
            else
            {
                sb.AppendLine("通过率: <code> --%</code>");
            }
            sb.AppendLine($"总计投稿: <code>{monthPost}</code>");

            await botClient.SendCommandReply(sb.ToString(), message, false, ParseMode.Html);
        }
    }
}
