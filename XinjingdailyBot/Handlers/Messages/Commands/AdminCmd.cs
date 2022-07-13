using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Helpers;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Handlers.Queries;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;
using System.Text;

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
