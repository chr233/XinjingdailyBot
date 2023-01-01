using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Infrastructure.Localization;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Command
{
    [AppService(ServiceLifetime = LifeTime.Scoped)]
    public class ReviewCommand
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly IChannelService _channelService;
        private readonly IPostService _postService;
        private readonly ITextHelperService _textHelperService;
        private readonly IMarkupHelperService _markupHelperService;
        private readonly IAttachmentService _attachmentService;

        public ReviewCommand(
            ITelegramBotClient botClient,
            IUserService userService,
            IChannelService channelService,
            IPostService postService,
            ITextHelperService textHelperService,
            IMarkupHelperService markupHelperService,
            IAttachmentService attachmentService)
        {
            _botClient = botClient;
            _userService = userService;
            _channelService = channelService;
            _postService = postService;
            _textHelperService = textHelperService;
            _markupHelperService = markupHelperService;
            _attachmentService = attachmentService;
        }

        /// <summary>
        /// 自定义拒绝稿件理由
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [TextCmd("NO", UserRights.ReviewPost, Description = "自定义拒绝稿件理由")]
        public async Task ResponseNo(Users dbUser, Message message, string[] args)
        {
            async Task<string> exec()
            {
                if (message.Chat.Id != _channelService.ReviewGroup.Id)
                {
                    return "该命令仅限审核群内使用";
                }

                if (message.ReplyToMessage == null)
                {
                    return "请回复审核消息并输入拒绝理由";
                }

                var messageId = message.ReplyToMessage.MessageId;

                var post = await _postService.Queryable().FirstAsync(x => x.ReviewMsgID == messageId || x.ManageMsgID == messageId);

                if (post == null)
                {
                    return "未找到稿件";
                }

                var reason = string.Join(' ', args).Trim();

                if (string.IsNullOrEmpty(reason))
                {
                    return "请输入拒绝理由";
                }

                post.Reason = RejectReason.CustomReason;
                await _postService.RejetPost(post, dbUser, reason);

                return $"已拒绝该稿件, 理由: {reason}";
            }

            var text = await exec();
            await _botClient.SendCommandReply(text, message, false);
        }

        /// <summary>
        /// 修改稿件文字说明
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [TextCmd("EDIT", UserRights.ReviewPost, Description = "修改稿件文字说明")]
        public async Task ResponseEditPost(Message message, string[] args)
        {
            async Task<string> exec()
            {
                if (message.Chat.Type != ChatType.Private && message.Chat.Id != _channelService.ReviewGroup.Id)
                {
                    return "该命令仅限审核群内使用";
                }

                if (message.ReplyToMessage == null)
                {
                    return "请回复审核消息并输入拒绝理由";
                }

                var messageId = message.ReplyToMessage.MessageId;

                var post = await _postService.Queryable().FirstAsync(x => x.ReviewMsgID == messageId || x.ManageMsgID == messageId);
                if (post == null)
                {
                    return "未找到稿件";
                }

                var postUser = await _userService.FetchUserByUserID(post.PosterUID);
                if (postUser == null)
                {
                    return "未找到投稿用户";
                }

                post.Text = string.Join(' ', args).Trim();
                await _postService.Updateable(post).UpdateColumns(x => new { x.Text }).ExecuteCommandAsync();

                return $"稿件描述已更新(投稿预览不会更新)";
            }

            var text = await exec();
            await _botClient.SendCommandReply(text, message, false);
        }

        /// <summary>
        /// 处理稿件
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        [QueryCmd("REVIEW", UserRights.ReviewPost, Alias = "REJECT", Description = "审核稿件")]
        public async Task HandleQuery(Users dbUser, CallbackQuery callbackQuery)
        {
            Message message = callbackQuery.Message!;
            Posts? post = await _postService.Queryable().FirstAsync(x => x.ManageMsgID == message.MessageId);

            if (post == null)
            {
                await _botClient.AutoReplyAsync("未找到稿件", callbackQuery);
                await _botClient.EditMessageReplyMarkupAsync(message, null);
                return;
            }

            if (post.Status != PostStatus.Reviewing)
            {
                await _botClient.AutoReplyAsync("请不要重复操作", callbackQuery);
                await _botClient.EditMessageReplyMarkupAsync(message, null);
                return;
            }

            if (!dbUser.Right.HasFlag(UserRights.ReviewPost))
            {
                await _botClient.AutoReplyAsync("无权操作", callbackQuery);
                return;
            }

            switch (callbackQuery.Data)
            {
                case "review reject":
                    await SwitchKeyboard(true, post, callbackQuery);
                    break;
                case "reject back":
                    await SwitchKeyboard(false, post, callbackQuery);
                    break;

                case "review tag nsfw":
                    await SetPostTag(post, BuildInTags.NSFW, callbackQuery);
                    break;
                case "review tag wanan":
                    await SetPostTag(post, BuildInTags.WanAn, callbackQuery);
                    break;
                case "review tag friend":
                    await SetPostTag(post, BuildInTags.Friend, callbackQuery);
                    break;
                case "review tag ai":
                    await SetPostTag(post, BuildInTags.AIGraph, callbackQuery);
                    break;

                case "reject fuzzy":
                    await RejectPostHelper(post, dbUser, RejectReason.Fuzzy);
                    break;
                case "reject duplicate":
                    await RejectPostHelper(post, dbUser, RejectReason.Duplicate);
                    break;
                case "reject boring":
                    await RejectPostHelper(post, dbUser, RejectReason.Boring);
                    break;
                case "reject confusing":
                    await RejectPostHelper(post, dbUser, RejectReason.Confused);
                    break;
                case "reject deny":
                    await RejectPostHelper(post, dbUser, RejectReason.Deny);
                    break;
                case "reject qrcode":
                    await RejectPostHelper(post, dbUser, RejectReason.QRCode);
                    break;
                case "reject other":
                    await RejectPostHelper(post, dbUser, RejectReason.Other);
                    break;

                case "review accept":
                    await AcceptPost(post, dbUser, callbackQuery);
                    break;

                case "review anymouse":
                    await SetAnymouse(post, callbackQuery);
                    break;

                case "review cancel":
                    await CancelPost(post, callbackQuery);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 设置或者取消匿名
        /// </summary>
        /// <param name="post"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private async Task SetAnymouse(Posts post, CallbackQuery query)
        {
            await _botClient.AutoReplyAsync("可以使用命令 /anymouse 切换默认匿名投稿", query);

            bool anonymous = !post.Anonymous;
            post.Anonymous = anonymous;
            post.ModifyAt = DateTime.Now;
            await _postService.Updateable(post).UpdateColumns(x => new { x.Anonymous, x.ModifyAt }).ExecuteCommandAsync();

            var keyboard = _markupHelperService.DirectPostKeyboard(anonymous, post.Tags);
            await _botClient.EditMessageReplyMarkupAsync(query.Message!, keyboard);
        }

        /// <summary>
        /// 取消投稿
        /// </summary>
        /// <param name="post"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private async Task CancelPost(Posts post, CallbackQuery query)
        {
            post.Status = PostStatus.Cancel;
            post.ModifyAt = DateTime.Now;
            await _postService.Updateable(post).UpdateColumns(x => new { x.Status, x.ModifyAt }).ExecuteCommandAsync();

            await _botClient.EditMessageTextAsync(query.Message!, Langs.PostCanceled, replyMarkup: null);

            await _botClient.AutoReplyAsync(Langs.PostCanceled, query);
        }

        /// <summary>
        /// 拒绝稿件包装方法
        /// </summary>
        /// <param name="post"></param>
        /// <param name="dbUser"></param>
        /// <param name="rejectReason"></param>
        /// <returns></returns>
        private async Task RejectPostHelper(Posts post, Users dbUser, RejectReason rejectReason)
        {
            post.Reason = rejectReason;
            string reason = _textHelperService.RejectReasonToString(rejectReason);
            await RejetPost(post, dbUser, reason);
        }

        /// <summary>
        /// 设置inlineKeyboard
        /// </summary>
        /// <param name="rejectMode"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private async Task SwitchKeyboard(bool rejectMode, Posts post, CallbackQuery callbackQuery)
        {
            if (rejectMode)
            {
                await _botClient.AutoReplyAsync("请选择拒稿原因", callbackQuery);
            }

            var keyboard = rejectMode ? _markupHelperService.ReviewKeyboardB() : _markupHelperService.ReviewKeyboardA(post.Tags);

            await _botClient.EditMessageReplyMarkupAsync(callbackQuery.Message!, keyboard);
        }

        /// <summary>
        /// 修改Tag
        /// </summary>
        /// <param name="post"></param>
        /// <param name="tag"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private async Task SetPostTag(Posts post, BuildInTags tag, CallbackQuery callbackQuery)
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
            if (post.Tags.HasFlag(BuildInTags.AIGraph))
            {
                tagNames.Add("AI怪图");
            }
            if (post.Tags == BuildInTags.None)
            {
                tagNames.Add("无");
            }

            post.ModifyAt = DateTime.Now;
            await _postService.Updateable(post).UpdateColumns(x => new { x.Tags, x.ModifyAt }).ExecuteCommandAsync();

            await _botClient.AutoReplyAsync(string.Join(' ', tagNames), callbackQuery);

            var keyboard = post.IsDirectPost ? _markupHelperService.DirectPostKeyboard(post.Anonymous, post.Tags) : _markupHelperService.ReviewKeyboardA(post.Tags);
            await _botClient.EditMessageReplyMarkupAsync(callbackQuery.Message!, keyboard);
        }

        /// <summary>
        /// 拒绝投稿
        /// </summary>
        /// <param name="post"></param>
        /// <param name="rejectReason"></param>
        /// <returns></returns>
        private async Task RejetPost(Posts post, Users dbUser, string rejectReason)
        {
            post.ReviewerUID = dbUser.UserID;
            post.Status = PostStatus.Rejected;
            post.ModifyAt = DateTime.Now;
            await _postService.Updateable(post).UpdateColumns(x => new { x.Reason, x.ReviewerUID, x.Status, x.ModifyAt }).ExecuteCommandAsync();

            Users poster = await _userService.Queryable().FirstAsync(x => x.UserID == post.PosterUID);

            //修改审核群消息
            string reviewMsg = _textHelperService.MakeReviewMessage(poster, dbUser, post.Anonymous, rejectReason);
            await _botClient.EditMessageTextAsync(_channelService.ReviewGroup.Id, (int)post.ManageMsgID, reviewMsg, parseMode: ParseMode.Html, disableWebPagePreview: true);

            //拒稿频道发布消息
            if (!post.IsMediaGroup)
            {
                await _botClient.CopyMessageAsync(_channelService.RejectChannel.Id, _channelService.ReviewGroup.Id, (int)post.ReviewMsgID);
            }
            else
            {
                var attachments = await _attachmentService.Queryable().Where(x => x.PostID == post.Id).ToListAsync();
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
                var _ = await _botClient.SendMediaGroupAsync(_channelService.RejectChannel.Id, group);
                //投稿消息组处理 TODO
            }

            //通知投稿人
            string posterMsg = _textHelperService.MakeNotification(rejectReason);
            if (poster.Notification)
            {
                await _botClient.SendTextMessageAsync(post.OriginChatID, posterMsg, replyToMessageId: (int)post.OriginMsgID, allowSendingWithoutReply: true);
            }
            else
            {
                await _botClient.EditMessageTextAsync(post.OriginChatID, (int)post.ActionMsgID, posterMsg);
            }

            poster.RejetCount++;
            poster.ModifyAt = DateTime.Now;
            await _userService.Updateable(poster).UpdateColumns(x => new { x.RejetCount, x.ModifyAt }).ExecuteCommandAsync();

            if (poster.UserID != dbUser.UserID) //非同一个人才增加审核数量
            {
                dbUser.ReviewCount++;
                dbUser.ModifyAt = DateTime.Now;
                await _userService.Updateable(dbUser).UpdateColumns(x => new { x.ReviewCount, x.ModifyAt }).ExecuteCommandAsync();
            }
        }

        /// <summary>
        /// 接受投稿
        /// </summary>
        /// <param name="post"></param>
        /// <param name="dbUser"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private async Task AcceptPost(Posts post, Users dbUser, CallbackQuery callbackQuery)
        {
            Users poster = await _userService.Queryable().FirstAsync(x => x.UserID == post.PosterUID);

            string postText = _textHelperService.MakePostText(post, poster);

            //发布频道发布消息
            if (!post.IsMediaGroup)
            {
                if (post.Tags.HasFlag(BuildInTags.NSFW))
                {
                    await _botClient.SendTextMessageAsync(_channelService.AcceptChannel.Id, _textHelperService.NSFWWrning, allowSendingWithoutReply: true);
                }

                Message msg;
                if (post.PostType == MessageType.Text)
                {
                    msg = await _botClient.SendTextMessageAsync(_channelService.AcceptChannel.Id, postText, ParseMode.Html, disableWebPagePreview: true);
                }
                else
                {
                    Attachments attachment = await _attachmentService.Queryable().FirstAsync(x => x.PostID == post.Id);

                    switch (post.PostType)
                    {
                        case MessageType.Photo:
                            msg = await _botClient.SendPhotoAsync(_channelService.AcceptChannel.Id, attachment.FileID, postText, ParseMode.Html);
                            break;
                        case MessageType.Audio:
                            msg = await _botClient.SendAudioAsync(_channelService.AcceptChannel.Id, attachment.FileID, postText, ParseMode.Html, title: attachment.FileName);
                            break;
                        case MessageType.Video:
                            msg = await _botClient.SendVideoAsync(_channelService.AcceptChannel.Id, attachment.FileID, caption: postText, parseMode: ParseMode.Html);
                            break;
                        case MessageType.Document:
                            msg = await _botClient.SendDocumentAsync(_channelService.AcceptChannel.Id, attachment.FileID, caption: postText, parseMode: ParseMode.Html);
                            break;
                        default:
                            await _botClient.AutoReplyAsync($"不支持的稿件类型: {post.PostType}", callbackQuery);
                            return;
                    }
                }
                post.PublicMsgID = msg.MessageId;
            }
            else
            {
                var attachments = await _attachmentService.Queryable().Where(x => x.PostID == post.Id).ToListAsync();
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
                    await _botClient.SendTextMessageAsync(_channelService.AcceptChannel.Id, _textHelperService.NSFWWrning, allowSendingWithoutReply: true);
                }

                var messages = await _botClient.SendMediaGroupAsync(_channelService.AcceptChannel.Id, group);
                post.PublicMsgID = messages.First().MessageId;
            }

            await _botClient.AutoReplyAsync("稿件已发布", callbackQuery);

            post.ReviewerUID = dbUser.UserID;
            post.Status = PostStatus.Accepted;
            post.ModifyAt = DateTime.Now;

            //修改审核群消息
            if (!post.IsDirectPost) // 非直接投稿
            {
                string reviewMsg = _textHelperService.MakeReviewMessage(poster, dbUser, post.Anonymous);
                await _botClient.EditMessageTextAsync(callbackQuery.Message!, reviewMsg, parseMode: ParseMode.Html, disableWebPagePreview: true);
            }
            else //直接投稿, 在审核群留档
            {
                string reviewMsg = _textHelperService.MakeReviewMessage(poster, post.PublicMsgID, post.Anonymous);
                var msg = await _botClient.SendTextMessageAsync(_channelService.ReviewGroup.Id, reviewMsg, parseMode: ParseMode.Html, disableWebPagePreview: true);
                post.ReviewMsgID = msg.MessageId;
            }

            await _postService.Updateable(post).UpdateColumns(x => new { x.ReviewMsgID, x.PublicMsgID, x.ReviewerUID, x.Status, x.ModifyAt }).ExecuteCommandAsync();

            //通知投稿人
            string posterMsg = _textHelperService.MakeNotification(post.IsDirectPost, post.PublicMsgID);

            if (poster.Notification && poster.UserID != dbUser.UserID)//启用通知并且审核与投稿不是同一个人
            {//单独发送通知消息
                await _botClient.SendTextMessageAsync(post.OriginChatID, posterMsg, ParseMode.Html, replyToMessageId: (int)post.OriginMsgID, allowSendingWithoutReply: true, disableWebPagePreview: true);
            }
            else
            {//静默模式, 不单独发送通知消息
                await _botClient.EditMessageTextAsync(post.OriginChatID, (int)post.ActionMsgID, posterMsg, ParseMode.Html, disableWebPagePreview: true);
            }

            //增加通过数量
            poster.AcceptCount++;
            poster.ModifyAt = DateTime.Now;
            await _userService.Updateable(poster).UpdateColumns(x => new { x.AcceptCount, x.ModifyAt }).ExecuteCommandAsync();

            if (!post.IsDirectPost) //增加审核数量
            {
                if (poster.UserID != dbUser.UserID)
                {
                    dbUser.ReviewCount++;
                    dbUser.ModifyAt = DateTime.Now;
                    await _userService.Updateable(dbUser).UpdateColumns(x => new { x.ReviewCount, x.ModifyAt }).ExecuteCommandAsync();
                }
            }
            else
            {
                poster.PostCount++;
                poster.ModifyAt = DateTime.Now;
                await _userService.Updateable(poster).UpdateColumns(x => new { x.PostCount, x.ModifyAt }).ExecuteCommandAsync();
            }
        }
    }
}
