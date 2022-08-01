using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Queries
{
    internal class CommonHandler
    {
        /// <summary>
        /// 处理CallbackQuery
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        internal static async Task HandleQuery(ITelegramBotClient botClient, Users dbUser, CallbackQuery callbackQuery)
        {
            Message message = callbackQuery.Message!;

            
        }

        

        /// <summary>
        /// 取消投稿
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="post"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        internal static async Task CancelPost(ITelegramBotClient botClient, Posts post, CallbackQuery callbackQuery)
        {
            post.Status = PostStatus.Cancel;
            post.ModifyAt = DateTime.Now;
            await DB.Updateable(post).UpdateColumns(x => new { x.Status, x.ModifyAt }).ExecuteCommandAsync();

            await botClient.EditMessageTextAsync(callbackQuery.Message!, "投稿已取消", replyMarkup: null);

            await botClient.AutoReplyAsync("投稿已取消", callbackQuery);
        }

        /// <summary>
        /// 确认投稿
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="post"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private static async Task Clear(ITelegramBotClient botClient, Posts post, Users dbUser, CallbackQuery callbackQuery)
        {
            Message reviewMsg;
            if (!post.IsMediaGroup)
            {
                reviewMsg = await botClient.ForwardMessageAsync(ReviewGroup.Id, post.OriginChatID, (int)post.OriginMsgID);
            }
            else
            {
                var attachments = await DB.Queryable<Attachments>().Where(x => x.PostID == post.Id).ToListAsync();
                var group = new IAlbumInputMedia[attachments.Count];
                for (int i = 0; i < attachments.Count; i++)
                {
                    MessageType attachmentType = attachments[i].Type;
                    if (attachmentType == MessageType.Unknown)
                    {
                        attachmentType = post.PostType;
                    }
                    group[i] = attachmentType switch
                    {
                        MessageType.Photo => new InputMediaPhoto(attachments[i].FileID),
                        MessageType.Audio => new InputMediaAudio(attachments[i].FileID),
                        MessageType.Video => new InputMediaVideo(attachments[i].FileID),
                        MessageType.Document => new InputMediaDocument(attachments[i].FileID),
                        _ => throw new Exception(),
                    };
                }
                var messages = await botClient.SendMediaGroupAsync(ReviewGroup.Id, group);
                reviewMsg = messages.First();
            }

            string msg = TextHelper.MakeReviewMessage(dbUser, post.Anymouse);

            var keyboard = MarkupHelper.ReviewKeyboardA(post.Tags);

            Message manageMsg = await botClient.SendTextMessageAsync(ReviewGroup.Id, msg, ParseMode.Html, disableWebPagePreview: true, replyToMessageId: reviewMsg.MessageId, replyMarkup: keyboard, allowSendingWithoutReply: true);

            post.ReviewMsgID = reviewMsg.MessageId;
            post.ManageMsgID = manageMsg.MessageId;
            post.Status = PostStatus.Reviewing;
            post.ModifyAt = DateTime.Now;
            await DB.Updateable(post).UpdateColumns(x => new { x.ReviewMsgID, x.ManageMsgID, x.Status, x.ModifyAt }).ExecuteCommandAsync();

            await botClient.AutoReplyAsync("稿件已投递", callbackQuery);
            if (dbUser.Notification)
            {
                await botClient.EditMessageTextAsync(callbackQuery.Message!, "感谢您的投稿, 审核结果将会稍后通知", replyMarkup: null);
            }
            else
            {
                await botClient.EditMessageTextAsync(callbackQuery.Message!, "感谢您的投稿, 已开启静默模式", replyMarkup: null);
            }

            dbUser.PostCount++;
            dbUser.ModifyAt = DateTime.Now;
            await DB.Updateable(dbUser).UpdateColumns(x => new { x.PostCount, x.ModifyAt }).ExecuteCommandAsync();
        }
    }
}
