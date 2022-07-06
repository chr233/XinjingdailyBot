using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Handlers.Queries;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Messages.Commands
{
    internal static class AdminCmd
    {

        internal static async Task<string> ResponseNo(ITelegramBotClient botClient, Users dbUser, Message message, string reason)
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

            reason = reason.Trim();

            if (string.IsNullOrEmpty(reason))
            {
                return "请输入拒绝理由";
            }

            post.Reason = RejectReason.CustomReason;
            await ReviewHandler.RejetPost(botClient, post, dbUser, reason);

            return "";
        }
    }
}
