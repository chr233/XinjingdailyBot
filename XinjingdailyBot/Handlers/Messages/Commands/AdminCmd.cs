using System.Text;
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
        /// 自定义通过
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


        internal static async Task<string> ResponseBan(ITelegramBotClient botClient, Users dbUser, Message message, string argv)
        {
            if (!dbUser.Right.HasFlag(UserRights.AdminCmd))
            {
                return "你没有管理权限";
            }

            if (message.Chat.Id != ReviewGroup.Id)
            {
                return "该命令仅限审核群内使用";
            }

            long UserID;
            string reason;

            if (message.ReplyToMessage != null)
            {
                int messageId = message.ReplyToMessage.MessageId;
                var post = await DB.Queryable<Posts>().FirstAsync(x => x.ReviewMsgID == messageId || x.ManageMsgID == messageId);
                if (post != null)
                {
                    UserID = post.PosterUID;
                }
                else
                {
                    return "你回复的信息不是稿件";
                }

                reason = argv;
            }
            else
            {
                string [] argvs = argv.Split(" ", 2);

                if (IsLong(argvs[0]))
                {
                    return "第一个字符串必须为用户ID";
                }

                UserID = long.Parse(argvs[0]);
                reason = argvs[1];
            }

            var target = await DB.Queryable<Ban>().FirstAsync(x => x.UserID == UserID);

            if (target == null)
            {
                target = new Ban();
                target.UserID = UserID;
                target.Reason = reason;
                target.BanTime = DateTime.Now;
                target.ExecutiveAdminID = message.From!.Id;

                await DB.Insertable(target).InsertColumns(x => new
                {
                    x.Reason,
                    x.BanTime,
                    x.UserID,
                    x.ExecutiveAdminID
                }).ExecuteCommandAsync();
            }
            else
            {
                target.Reason = reason;
                target.BanTime = DateTime.Now;
                target.ExecutiveAdminID = message.From!.Id;

                await DB.Updateable(target).UpdateColumns(x => new
                {
                    x.Reason,
                    x.BanTime,
                    x.UserID,
                    x.ExecutiveAdminID
                }).ExecuteCommandAsync();
            }

            return "已封禁该用户!\n理由: <code>{reason}</code>";

        }

        internal static async Task<string> ResponseUnban(ITelegramBotClient botClient, Users dbUser, Message message, string argv)
        {
            if (!dbUser.Right.HasFlag(UserRights.AdminCmd))
            {
                return "你没有管理权限";
            }

            if (message.Chat.Id != ReviewGroup.Id)
            {
                return "该命令仅限审核群内使用";
            }

            long userID;
            string reason;

            if (message.ReplyToMessage != null)
            {
                int messageId = message.ReplyToMessage.MessageId;
                var post = await DB.Queryable<Posts>().FirstAsync(x => x.ReviewMsgID == messageId || x.ManageMsgID == messageId);
                if (post != null)
                {
                    userID = post.PosterUID;
                }
                else
                {
                    return "你回复的信息不是稿件";
                }

                reason = argv;
            }
            else
            {
                string [] argvs = argv.Split(" ", 2);

                if (IsLong(argvs[0]))
                {
                    return "第一个字符串必须为用户ID";
                }

                userID = long.Parse(argvs[0]);
                reason = argvs[1];
            }

            var target = await DB.Queryable<Ban>().FirstAsync(x => x.UserID == userID);

            if (target == null)
            {
                return "该用户未被封禁";
            }

            DB.Deleteable(target).ExecuteCommand();

            return $"已解封用户 {TextHelper.HtmlUserLink(await DB.Queryable<Users>().FirstAsync(x => x.Id == target.UserID))}\n" +
                   $"理由: <code>{reason}</code>";
        }

        internal static async Task<string> QueryBan(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            return await QueryBan(botClient, dbUser, message, null);
        }

        internal static async Task<string> QueryBan(ITelegramBotClient botClient, Users dbUser, Message message, string? argv)
        {
            if (!dbUser.Right.HasFlag(UserRights.AdminCmd))
            {
                return "你没有管理权限";
            }

            if (message.Chat.Id != ReviewGroup.Id)
            {
                return "该命令仅限审核群内使用";
            }

            long targetID;

            if (argv != null)
            {
                if (!IsLong(argv))
                {
                    return "参数必须为一个数字";
                }
                targetID = long.Parse(argv);
            }
            else
            {
                int messageId = message.ReplyToMessage!.MessageId;
                var post = await DB.Queryable<Posts>().FirstAsync(x => x.ReviewMsgID == messageId || x.ManageMsgID == messageId);
                if (post == null)
                {
                    return "未找到此投稿";
                }
                targetID = post.PosterUID;
            }

            var ban = await IsBan(targetID);
            if (ban == null)
            {
                return "该用户未被封禁";
            }

            return $"被封禁人: {TextHelper.HtmlUserLink(await DB.Queryable<Users>().FirstAsync(x => x.Id == ban.UserID))}\n" +
                   $"封禁管理: {TextHelper.HtmlUserLink(await DB.Queryable<Users>().FirstAsync(x => x.Id == ban.ExecutiveAdminID))}\n" +
                   $"封禁时间: {ban.BanTime.ToString("yyyy MMMM dd")}\n" +
                   $"封禁理由: <code>{ban.Reason}</code>";
        }

        /// <summary>
        /// 自定义通过
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="reason"></param>
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
