using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Queries
{
    internal class PostHandler
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

            Posts? post = await DB.Queryable<Posts>().FirstAsync(x => x.ActionMsgID == message.MessageId);

            if (post == null)
            {
                await botClient.AutoReplyAsync("未找到稿件", callbackQuery);
                await botClient.EditMessageReplyMarkupAsync(message, null);
                return;
            }

            if (post.Status != PostStatus.Padding)
            {
                await botClient.AutoReplyAsync("请不要重复操作", callbackQuery);
                await botClient.EditMessageReplyMarkupAsync(message, null);
                return;
            }

            if (post.PosterUID != callbackQuery.From.Id)
            {
                await botClient.AutoReplyAsync("这不是你的稿件", callbackQuery);
                return;
            }

            switch (callbackQuery.Data)
            {
                case "post anymouse":
                    await SetAnymouse(botClient, post, dbUser, callbackQuery);
                    break;
                case "post cancel":
                    await CancelPost(botClient, post, callbackQuery);
                    break;
                case "post confirm":
                    await ConfirmPost(botClient, post, dbUser, callbackQuery);
                    break;
            }
        }

        /// <summary>
        /// 设置或者取消匿名
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="post"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        internal static async Task SetAnymouse(ITelegramBotClient botClient, Posts post, Users dbUser, CallbackQuery callbackQuery)
        {
            await botClient.AutoReplyAsync("可以使用命令 /anymouse 切换默认匿名投稿", callbackQuery);

            bool anymouse = !post.Anymouse;
            post.Anymouse = anymouse;
            post.ModifyAt = DateTime.Now;
            await DB.Updateable(post).UpdateColumns(x => new { x.Anymouse, x.ModifyAt }).ExecuteCommandAsync();

            var keyboard = post.IsDirectPost ? MarkupHelper.DirectPostKeyboard(anymouse, post.Tags) : MarkupHelper.PostKeyboard(anymouse);
            await botClient.EditMessageReplyMarkupAsync(callbackQuery.Message!, keyboard);
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
        private static async Task ConfirmPost(ITelegramBotClient botClient, Posts post, Users dbUser, CallbackQuery callbackQuery)
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
