using XinjingdailyBot.Handlers.Messages;
using XinjingdailyBot.Helpers;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Queries
{
    internal static class ReviewHandler
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
                    await SwitchKeyboard(botClient, true, post, callbackQuery);
                    break;
                case "reject back":
                    await SwitchKeyboard(botClient, false, post, callbackQuery);
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
                    await RejectPostHelper(botClient, post, dbUser, RejectReason.Fuzzy);
                    break;
                case "reject duplicate":
                    await RejectPostHelper(botClient, post, dbUser, RejectReason.Duplicate);
                    break;
                case "reject boring":
                    await RejectPostHelper(botClient, post, dbUser, RejectReason.Boring);
                    break;
                case "reject confusing":
                    await RejectPostHelper(botClient, post, dbUser, RejectReason.Confused);
                    break;
                case "reject deny":
                    await RejectPostHelper(botClient, post, dbUser, RejectReason.Deny);
                    break;
                case "reject qrcode":
                    await RejectPostHelper(botClient, post, dbUser, RejectReason.QRCode);
                    break;
                case "reject other":
                    await RejectPostHelper(botClient, post, dbUser, RejectReason.Other);
                    break;

                case "review accept":
                    await AcceptPost(botClient, post, dbUser, callbackQuery);
                    break;

                case "review anymouse":
                    await PostHandler.SetAnymouse(botClient, post, callbackQuery);
                    break;

                case "review cancel":
                    await PostHandler.CancelPost(botClient, post, callbackQuery);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 拒绝稿件包装方法
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="post"></param>
        /// <param name="dbUser"></param>
        /// <param name="rejectReason"></param>
        /// <returns></returns>
        private static async Task RejectPostHelper(ITelegramBotClient botClient, Posts post, Users dbUser, RejectReason rejectReason)
        {
            post.Reason = rejectReason;
            string reason = TextHelper.RejectReasonToString(rejectReason);
            await RejetPost(botClient, post, dbUser, reason);

        }

        /// <summary>
        /// 设置inlineKeyboard
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="rejectMode"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private static async Task SwitchKeyboard(ITelegramBotClient botClient, bool rejectMode, Posts post, CallbackQuery callbackQuery)
        {
            if (rejectMode)
            {
                await botClient.AutoReplyAsync("请选择拒稿原因", callbackQuery);
            }

            var keyboard = rejectMode ? MarkupHelper.ReviewKeyboardB() : MarkupHelper.ReviewKeyboardA(post.Tags);

            await botClient.EditMessageReplyMarkupAsync(callbackQuery.Message!, keyboard);
        }

        /// <summary>
        /// 修改Tag
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="post"></param>
        /// <param name="tag"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
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

            List<string> tagNames = new() { "当前标签:" };
            if (post.Tags.HasFlag(BuildInTags.NSFW))
            {
                tagNames.Add("NSFW");
            }
            if (post.Tags.HasFlag(BuildInTags.WanAn))
            {
                tagNames.Add("晚安");
            }
            if (post.Tags.HasFlag(BuildInTags.Friend))
            {
                tagNames.Add("我有一个朋友");
            }
            if (post.Tags == BuildInTags.None)
            {
                tagNames.Add("无");
            }

            post.ModifyAt = DateTime.Now;
            await DB.Updateable(post).UpdateColumns(x => new { x.Tags, x.ModifyAt }).ExecuteCommandAsync();

            await botClient.AutoReplyAsync(string.Join(' ', tagNames), callbackQuery);

            var keyboard = post.IsDirectPost ? MarkupHelper.DirectPostKeyboard(post.Anymouse, post.Tags) : MarkupHelper.ReviewKeyboardA(post.Tags);
            await botClient.EditMessageReplyMarkupAsync(callbackQuery.Message!, keyboard);
        }

        /// <summary>
        /// 拒绝投稿
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="post"></param>
        /// <param name="rejectReason"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        internal static async Task RejetPost(ITelegramBotClient botClient, Posts post, Users dbUser, string rejectReason)
        {
            post.ReviewerUID = dbUser.UserID;
            post.Status = PostStatus.Rejected;
            post.ModifyAt = DateTime.Now;
            await DB.Updateable(post).UpdateColumns(x => new { x.Reason, x.ReviewerUID, x.Status, x.ModifyAt }).ExecuteCommandAsync();

            Users poster = await DB.Queryable<Users>().FirstAsync(x => x.UserID == post.PosterUID);

            //修改审核群消息
            string reviewMsg = TextHelper.MakeReviewMessage(poster, dbUser, post.Anymouse, rejectReason);
            await botClient.EditMessageTextAsync(ReviewGroup.Id, (int)post.ManageMsgID, reviewMsg, parseMode: ParseMode.Html, disableWebPagePreview: true);

            //拒稿频道发布消息
            if (!post.IsMediaGroup)
            {
                await botClient.CopyMessageAsync(RejectChannel.Id, ReviewGroup.Id, (int)post.ReviewMsgID);
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
                var messages = await botClient.SendMediaGroupAsync(RejectChannel.Id, group);
            }

            //通知投稿人
            string posterMsg = TextHelper.MakeNotification(rejectReason);
            if (poster.Notification)
            {
                await botClient.SendTextMessageAsync(post.OriginChatID, posterMsg, replyToMessageId: (int)post.OriginMsgID, allowSendingWithoutReply: true);
            }
            else
            {
                await botClient.EditMessageTextAsync(post.OriginChatID, (int)post.ActionMsgID, posterMsg);
            }

            poster.RejetCount++;
            poster.ModifyAt = DateTime.Now;
            await DB.Updateable(poster).UpdateColumns(x => new { x.RejetCount, x.ModifyAt }).ExecuteCommandAsync();

            if (poster.UserID != dbUser.UserID) //非同一个人才增加审核数量
            {
                dbUser.ReviewCount++;
                dbUser.ModifyAt = DateTime.Now;
                await DB.Updateable(dbUser).UpdateColumns(x => new { x.ReviewCount, x.ModifyAt }).ExecuteCommandAsync();
            }
        }

        /// <summary>
        /// 接受投稿
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="post"></param>
        /// <param name="dbUser"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private static async Task AcceptPost(ITelegramBotClient botClient, Posts post, Users dbUser, CallbackQuery callbackQuery)
        {
            Users poster = await DB.Queryable<Users>().FirstAsync(x => x.UserID == post.PosterUID);

            string postText = TextHelper.MakePostText(post, poster);

            //发布频道发布消息
            if (!post.IsMediaGroup)
            {
                if (post.Tags.HasFlag(BuildInTags.NSFW))
                {
                    await botClient.SendTextMessageAsync(AcceptChannel.Id, MessageDispatcher.NSFWWrning, allowSendingWithoutReply: true);
                }

                Message msg;
                if (post.PostType == MessageType.Text)
                {
                    msg = await botClient.SendTextMessageAsync(AcceptChannel.Id, postText, ParseMode.Html, disableWebPagePreview: true);
                }
                else
                {
                    Attachments attachment = await DB.Queryable<Attachments>().FirstAsync(x => x.PostID == post.Id);

                    switch (post.PostType)
                    {
                        case MessageType.Photo:
                            msg = await botClient.SendPhotoAsync(AcceptChannel.Id, attachment.FileID, postText, ParseMode.Html);
                            break;
                        case MessageType.Audio:
                            msg = await botClient.SendAudioAsync(AcceptChannel.Id, attachment.FileID, postText, ParseMode.Html, title: attachment.FileName);
                            break;
                        case MessageType.Video:
                            msg = await botClient.SendVideoAsync(AcceptChannel.Id, attachment.FileID, caption: postText, parseMode: ParseMode.Html);
                            break;
                        case MessageType.Document:
                            msg = await botClient.SendDocumentAsync(AcceptChannel.Id, attachment.FileID, caption: postText, parseMode: ParseMode.Html);
                            break;
                        default:
                            await botClient.AutoReplyAsync($"不支持的稿件类型: {post.PostType}", callbackQuery);
                            return;
                    }
                }
                post.PublicMsgID = msg.MessageId;
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
                        MessageType.Photo => new InputMediaPhoto(attachments[i].FileID) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                        MessageType.Audio => new InputMediaAudio(attachments[i].FileID) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                        MessageType.Video => new InputMediaVideo(attachments[i].FileID) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                        MessageType.Document => new InputMediaDocument(attachments[i].FileID) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                        _ => throw new Exception(),
                    };
                }

                if (post.Tags.HasFlag(BuildInTags.NSFW))
                {
                    await botClient.SendTextMessageAsync(AcceptChannel.Id, MessageDispatcher.NSFWWrning, allowSendingWithoutReply: true);
                }

                var messages = await botClient.SendMediaGroupAsync(AcceptChannel.Id, group);
                post.PublicMsgID = messages.First().MessageId;
            }

            await botClient.AutoReplyAsync("稿件已发布", callbackQuery);

            post.ReviewerUID = dbUser.UserID;
            post.Status = PostStatus.Accepted;
            post.ModifyAt = DateTime.Now;

            //修改审核群消息
            if (!post.IsDirectPost) // 非直接投稿
            {
                string reviewMsg = TextHelper.MakeReviewMessage(poster, dbUser, post.Anymouse);
                await botClient.EditMessageTextAsync(callbackQuery.Message!, reviewMsg, parseMode: ParseMode.Html, disableWebPagePreview: true);
            }
            else //直接投稿, 在审核群留档
            {
                string reviewMsg = TextHelper.MakeReviewMessage(poster, post.PublicMsgID, post.Anymouse);
                var msg = await botClient.SendTextMessageAsync(ReviewGroup.Id, reviewMsg, parseMode: ParseMode.Html, disableWebPagePreview: true);
                post.ReviewMsgID = msg.MessageId;
            }

            await DB.Updateable(post).UpdateColumns(x => new { x.ReviewMsgID, x.PublicMsgID, x.ReviewerUID, x.Status, x.ModifyAt }).ExecuteCommandAsync();

            //通知投稿人
            bool directPost = post.ManageMsgID == post.ActionMsgID;

            string posterMsg = TextHelper.MakeNotification(post.IsDirectPost, post.PublicMsgID);

            if (poster.Notification && poster.UserID != dbUser.UserID)//启用通知并且审核与投稿不是同一个人
            {//单独发送通知消息
                await botClient.SendTextMessageAsync(post.OriginChatID, posterMsg, ParseMode.Html, replyToMessageId: (int)post.OriginMsgID, allowSendingWithoutReply: true, disableWebPagePreview: true);
            }
            else
            {//静默模式, 不单独发送通知消息
                await botClient.EditMessageTextAsync(post.OriginChatID, (int)post.ActionMsgID, posterMsg, ParseMode.Html, disableWebPagePreview: true);
            }

            //增加通过数量
            poster.AcceptCount++;
            poster.ModifyAt = DateTime.Now;
            await DB.Updateable(poster).UpdateColumns(x => new { x.AcceptCount, x.ModifyAt }).ExecuteCommandAsync();

            if (!post.IsDirectPost) //增加审核数量
            {
                if (poster.UserID != dbUser.UserID)
                {
                    dbUser.ReviewCount++;
                    dbUser.ModifyAt = DateTime.Now;
                    await DB.Updateable(dbUser).UpdateColumns(x => new { x.ReviewCount, x.ModifyAt }).ExecuteCommandAsync();
                }
            }
            else
            {
                poster.PostCount++;
                poster.ModifyAt = DateTime.Now;
                await DB.Updateable(poster).UpdateColumns(x => new { x.PostCount, x.ModifyAt }).ExecuteCommandAsync();
            }
        }
    }
}
