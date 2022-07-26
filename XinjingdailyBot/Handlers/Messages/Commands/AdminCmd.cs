using System.Text;
using SqlSugar;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Handlers.Queries;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Messages.Commands
{
    internal static class AdminCmd
    {
        /// <summary>
        /// 审核命令帮助
        /// </summary>
        /// <param name="dbUser"></param>
        /// <returns></returns>
        internal static async Task<string?> ResponseReviewHelp(Users dbUser)
        {
            return "";
        }

        /// <summary>
        /// 自定义拒稿
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        internal static async Task<string> ResponseNo(ITelegramBotClient botClient, Users dbUser, Message message, string? reason)
        {
            if (!dbUser.Right.HasFlag(UserRights.ReviewPost))
            {
                return "该命令需要具有审核权限才可以使用";
            }

            if (message.Chat.Id != ReviewGroup.Id)
            {
                return "该命令仅限审核群内使用";
            }

            if (message.ReplyToMessage == null)
            {
                return "请回复审核消息并输入拒绝理由";
            }

            int messageId = message.ReplyToMessage.MessageId;

            var post = await DB.Queryable<Posts>().FirstAsync(x => x.ReviewMsgID == messageId || x.ManageMsgID == messageId);

            if (post == null)
            {
                return "未找到稿件";
            }

            reason = reason?.Trim();

            if (string.IsNullOrEmpty(reason))
            {
                return "请输入拒绝理由";
            }

            post.Reason = RejectReason.CustomReason;
            await ReviewHandler.RejetPost(botClient, post, dbUser, reason);

            return $"已拒绝该稿件, 理由: {reason}";
        }

        /// <summary>
        /// 自定义通过(TODO)
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        internal static async Task<string> ResponseYes(ITelegramBotClient botClient, Users dbUser, Message message, string? reason)
        {
            if (!dbUser.Right.HasFlag(UserRights.ReviewPost))
            {
                return "该命令需要具有审核权限才可以使用";
            }

            if (message.Chat.Id != ReviewGroup.Id)
            {
                return "该命令仅限审核群内使用";
            }

            if (message.ReplyToMessage == null)
            {
                return "请回复审核消息并输入拒绝理由";
            }

            int messageId = message.ReplyToMessage.MessageId;

            var post = await DB.Queryable<Posts>().FirstAsync(x => x.ReviewMsgID == messageId || x.ManageMsgID == messageId);

            if (post == null)
            {
                return "未找到稿件";
            }

            reason = reason?.Trim();

            if (string.IsNullOrEmpty(reason))
            {
                return "请输入拒绝理由";
            }

            post.Reason = RejectReason.CustomReason;
            await ReviewHandler.RejetPost(botClient, post, dbUser, reason);

            return $"已拒绝该稿件, 理由: {reason}";
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task<string> ResponseUserInfo(ITelegramBotClient botClient, Users dbUser, Message message, string[]? args)
        {
            var targetUser = await FetchUserHelper.FetchTargetUser(message);

            if (targetUser == null)
            {
                if (args != null && args.Any())
                {
                    targetUser = await FetchUserHelper.FetchTargetUser(args.First());
                }
            }

            if (targetUser == null)
            {
                return "找不到指定用户, 你可以手动指定用户名/用户ID";
            }


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

            StringBuilder sb = new();
            sb.AppendLine($"用户名: <code>{userNick}</code>");
            sb.AppendLine($"用户ID: <code>{targetUser.UserID}</code>");
            sb.AppendLine($"用户组: <code>{group}</code>");
            sb.AppendLine($"等级:  <code>{level}</code>");
            sb.AppendLine($"投稿数量: <code>{targetUser.PostCount}</code>");
            sb.AppendLine($"通过数量: <code>{targetUser.AcceptCount}</code>");
            sb.AppendLine($"拒绝数量: <code>{targetUser.RejetCount}</code>");
            sb.AppendLine($"审核数量: <code>{targetUser.ReviewCount}</code>");
            return sb.ToString();
        }



        /// <summary>
        /// 封禁用户
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task<string> ResponseBan(ITelegramBotClient botClient, Users dbUser, Message message, string[]? args)
        {
            var targetUser = await FetchUserHelper.FetchTargetUser(message);

            if (targetUser == null)
            {
                if (args != null && args.Any())
                {
                    targetUser = await FetchUserHelper.FetchTargetUser(args.First());
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

            string reason = args != null ? string.Join(' ', args) : "【未指定理由】";

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

        /// <summary>
        /// 解封用户
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task<string> ResponseUnban(ITelegramBotClient botClient, Users dbUser, Message message, string[]? args)
        {
            var targetUser = await FetchUserHelper.FetchTargetUser(message);

            if (targetUser == null)
            {
                if (args != null && args.Any())
                {
                    targetUser = await FetchUserHelper.FetchTargetUser(args.First());
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

            string reason = args != null ? string.Join(' ', args) : "【未指定理由】";

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

        /// <summary>
        /// 查询封禁记录
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task<string> ResponseQueryBan(ITelegramBotClient botClient, Users dbUser, Message message, string[]? args)
        {
            var targetUser = await FetchUserHelper.FetchTargetUser(message);

            if (targetUser == null)
            {
                if (args != null && args.Any())
                {
                    targetUser = await FetchUserHelper.FetchTargetUser(args.First());
                    args = args[1..];
                }
            }

            if (targetUser == null)
            {
                return "找不到指定用户";
            }

            var records = await DB.Queryable<BanRecords>().Where(x => x.UserID == targetUser.UserID).ToListAsync();

            StringBuilder sb = new();

            string status = targetUser.IsBan ? "已封禁" : "正常";
            sb.AppendLine($"用户名: <code>{targetUser.UserNick}</code>");
            sb.AppendLine($"用户ID: <code>{targetUser.UserID}</code>");
            sb.AppendLine($"状态: <code>{status}</code>");
            sb.AppendLine();

            if (records == null || !records.Any())
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

            return sb.ToString();
        }

        /// <summary>
        /// 获取群组信息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static string ResponseGroupInfo(Users dbUser, Message message)
        {
            var chat = message.Chat;

            if (chat.Type != ChatType.Group && chat.Type != ChatType.Supergroup)
            {
                return "该命令仅限群组内使用";
            }

            StringBuilder sb = new();
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

            return sb.ToString();
        }
    }
}
