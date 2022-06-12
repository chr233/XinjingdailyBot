using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Models;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Helpers;
using Telegram.Bot.Types.Enums;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Queries
{
    internal class PostHandler
    {
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
                    await SetAnymouse(botClient, post, callbackQuery);
                    break;
                case "post cancel":
                    await CancelPost(botClient, post, callbackQuery);
                    break;
                case "post confirm":
                    await ConfirmPost(botClient, post, callbackQuery);
                    break;
            }
        }

        /// <summary>
        /// 设置或者取消匿名
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="post"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static async Task SetAnymouse(ITelegramBotClient botClient, Posts post, CallbackQuery callbackQuery)
        {
            bool anymouse = !post.Anymouse;
            post.Anymouse = anymouse;
            post.ModifyAt = DateTime.Now;
            await DB.Updateable(post).UpdateColumns(x => new { x.Anymouse, x.ModifyAt }).ExecuteCommandAsync();

            var keyboard = MarkupHelper.PostKeyboard(anymouse);

            await botClient.EditMessageReplyMarkupAsync(callbackQuery.Message!, keyboard);

            await botClient.AutoReplyAsync("可以使用命令 /perfect_anymouse 切换默认匿名投稿", callbackQuery);
        }

        /// <summary>
        /// 取消投稿
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="post"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private static async Task CancelPost(ITelegramBotClient botClient, Posts post, CallbackQuery callbackQuery)
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
        private static async Task ConfirmPost(ITelegramBotClient botClient, Posts post, CallbackQuery callbackQuery)
        {
            Message reviewMsg = await botClient.ForwardMessageAsync(ReviewGroup.Id, post.OriginChatID, (int)post.OriginMsgID);

            string msg = string.Join('\n', "投稿人", "222");

            var keyboard = MarkupHelper.ReviewKeyboardA();

            Message manageMsg = await botClient.SendTextMessageAsync(ReviewGroup.Id, msg, ParseMode.Html, replyToMessageId: reviewMsg.MessageId, replyMarkup: keyboard);

            post.ReviewMsgID = reviewMsg.MessageId;
            post.ManageMsgID = manageMsg.MessageId;
            post.Status = PostStatus.Reviewing;
            post.ModifyAt = DateTime.Now;
            await DB.Updateable(post).UpdateColumns(x => new { x.ReviewMsgID, x.ManageMsgID, x.Status, x.ModifyAt }).ExecuteCommandAsync();

            await botClient.EditMessageTextAsync(callbackQuery.Message!, "稿件已投递", replyMarkup: null);

            await botClient.AutoReplyAsync("稿件已投递", callbackQuery);
        }

    }
}
