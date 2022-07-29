using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Handlers.Queries;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Messages.Commands
{
    internal static class ReviewCmd
    {
        /// <summary>
        /// 自定义拒稿
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task ResponseNo(ITelegramBotClient botClient, Users dbUser, Message message, string[] args)
        {
            async Task<string> exec()
            {
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

                string reason = string.Join(' ', args).Trim();

                if (string.IsNullOrEmpty(reason))
                {
                    return "请输入拒绝理由";
                }

                post.Reason = RejectReason.CustomReason;
                await ReviewHandler.RejetPost(botClient, post, dbUser, reason);

                return $"已拒绝该稿件, 理由: {reason}";
            }

            string text = await exec();
            await botClient.SendCommandReply(text, message,false);
        }

        /// <summary>
        /// 修改稿件文字说明
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task ResponseEditPost(ITelegramBotClient botClient, Users dbUser, Message message, string[] args)
        {
            async Task<string> exec()
            {
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

                var postUser = await FetchUserHelper.FetchDbUser(post.PosterUID);
                if (postUser == null)
                {
                    return "未找到投稿用户";
                }

                post.Text = string.Join(' ', args).Trim();
                await DB.Updateable(post).UpdateColumns(x => new { x.Text }).ExecuteCommandAsync();

                return $"稿件描述已更新(投稿预览不会更新)";
            }

            string text = await exec();
            await botClient.SendCommandReply(text, message, false);
        }
    }
}
