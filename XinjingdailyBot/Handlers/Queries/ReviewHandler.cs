using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Models;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Helpers;
using Telegram.Bot.Types.Enums;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Queries
{
    internal class ReviewHandler
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

            Posts? post = await DB.Queryable<Posts>().FirstAsync(x => x.ManageMsgID == message.MessageId);

            if (post == null)
            {
                await botClient.AutoReplyAsync("未找到稿件", callbackQuery);
                await botClient.EditMessageReplyMarkupAsync(message, null);
                return;
            }

            if (post.Status != PostStatus.Reviewing)
            {
                await botClient.AutoReplyAsync("请不要重复操作", callbackQuery);
                await botClient.EditMessageReplyMarkupAsync(message, null);
                return;
            }

            if (!dbUser.Right.HasFlag(UserRights.ReviewPost))
            {
                await botClient.AutoReplyAsync("无权操作", callbackQuery);
                return;
            }

            switch (callbackQuery.Data)
            {
                case "review reject":
                    await SwitchKeyboard(botClient, true, callbackQuery);
                    break;
                case "reject back":
                    await SwitchKeyboard(botClient, false, callbackQuery);
                    break;

                case "review tag nsfw":
                    await SetPostTag(botClient, post, BuildInTags.NSFW, callbackQuery);
                    break;
                case "review tag wanan":
                    await SetPostTag(botClient, post, BuildInTags.WanAn, callbackQuery);
                    break;
                case "review tag friend":
                    await SetPostTag(botClient, post, BuildInTags.Friend, callbackQuery);
                    break;

                case "reject fuzzy":
                case "reject duplicate":
                case "reject boring":
                    break;

                case "review accept":
                    break;
            }
        }

        /// <summary>
        /// 设置inlineKeyboard
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="rejectMode"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private static async Task SwitchKeyboard(ITelegramBotClient botClient, bool rejectMode, CallbackQuery callbackQuery)
        {
            var keyboard = rejectMode ? MarkupHelper.ReviewKeyboardB() : MarkupHelper.ReviewKeyboardA();

            await botClient.EditMessageReplyMarkupAsync(callbackQuery.Message!, keyboard);

            if (rejectMode)
            {
                await botClient.AutoReplyAsync("请选择拒稿原因", callbackQuery);
            }
        }

        private static async Task SetPostTag(ITelegramBotClient botClient, Posts post, BuildInTags tag, CallbackQuery callbackQuery)
        {
            if (post.Tags.HasFlag(tag))
            {
                post.Tags &= ~tag;
            }
            else
            {
                post.Tags |= tag;
            }

            post.ModifyAt = DateTime.Now;

            await DB.Updateable(post).UpdateColumns(x => new { x.Tags, x.ModifyAt }).ExecuteCommandAsync();

            await botClient.AutoReplyAsync(post.Tags.ToString(), callbackQuery);
        }
    }
}
