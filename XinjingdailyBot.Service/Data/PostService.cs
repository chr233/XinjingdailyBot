using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
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
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Service.Data
{
    [AppService(ServiceType = typeof(IPostService), ServiceLifetime = LifeTime.Singleton)]
    public sealed class PostService : BaseService<Posts>, IPostService
    {
        private readonly ILogger<PostService> _logger;
        private readonly AttachmentRepository _attachmentRepository;
        private readonly IChannelService _channelService;
        private readonly IChannelOptionService _channelOptionService;
        private readonly ITextHelperService _textHelperService;
        private readonly IMarkupHelperService _markupHelperService;
        private readonly IAttachmentHelperService _attachmentHelperService;
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly IAttachmentService _attachmentService;

        public PostService(
            ILogger<PostService> logger,
            AttachmentRepository attachmentRepository,
            IChannelService channelService,
            IChannelOptionService channelOptionService,
            ITextHelperService textHelperService,
            IMarkupHelperService markupHelperService,
            IAttachmentHelperService attachmentHelperService,
            ITelegramBotClient botClient,
            IUserService userService,
            IAttachmentService attachmentService)
        {
            _logger = logger;
            _attachmentRepository = attachmentRepository;
            _channelService = channelService;
            _channelOptionService = channelOptionService;
            _textHelperService = textHelperService;
            _markupHelperService = markupHelperService;
            _attachmentHelperService = attachmentHelperService;
            _botClient = botClient;
            _userService = userService;
            _attachmentService = attachmentService;
        }

        /// <summary>
        /// 文字投稿长度上限
        /// </summary>
        private static readonly int MaxPostText = 2000;

        /// <summary>
        /// 处理文字投稿
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task HandleTextPosts(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            if (!dbUser.Right.HasFlag(UserRights.SendPost))
            {
                await botClient.AutoReplyAsync(Langs.NoPostRight, message);
                return;
            }
            if (_channelService.ReviewGroup.Id == -1)
            {
                await botClient.AutoReplyAsync(Langs.ReviewGroupNotSet, message);
                return;
            }

            if (string.IsNullOrEmpty(message.Text))
            {
                await botClient.AutoReplyAsync(Langs.TextPostCantBeNull, message);
                return;
            }

            if (message.Text!.Length > MaxPostText)
            {
                await botClient.AutoReplyAsync($"文本长度超过上限 {MaxPostText}, 无法创建投稿", message);
                return;
            }

            ChannelOption channelOption = ChannelOption.Normal;
            string? channelName = null, channelTitle = null;
            if (message.ForwardFromChat?.Type == ChatType.Channel)
            {
                long channelId = message.ForwardFromChat.Id;
                channelName = $"{message.ForwardFromChat.Username}/{message.ForwardFromMessageId}";
                channelTitle = message.ForwardFromChat.Title;
                channelOption = await _channelOptionService.FetchChannelOption(channelId, channelName, channelTitle);
            }

            BuildInTags tags = _textHelperService.FetchTags(message.Text);
            string text = _textHelperService.ParseMessage(message);

            bool anonymous = dbUser.PreferAnonymous;

            //直接发布模式
            bool directPost = dbUser.Right.HasFlag(UserRights.DirectPost);
            //发送确认消息
            var keyboard = directPost ? _markupHelperService.DirectPostKeyboard(anonymous, tags) : _markupHelperService.PostKeyboard(anonymous);
            string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";

            //生成数据库实体
            Posts newPost = new()
            {
                Anonymous = anonymous,
                Text = text,
                RawText = message.Text ?? "",
                ChannelName = channelName ?? "",
                ChannelTitle = channelTitle ?? "",
                Status = directPost ? PostStatus.Reviewing : PostStatus.Padding,
                PostType = message.Type,
                Tags = tags,
                PosterUID = dbUser.UserID
            };

            //套用频道设定
            switch (channelOption)
            {
                case ChannelOption.Normal:
                    break;
                case ChannelOption.PurgeOrigin:
                    postText += "\n由于系统设定, 来自该频道的投稿将不会显示来源";
                    newPost.ChannelName += '~';
                    break;
                case ChannelOption.AutoReject:
                    postText = "由于系统设定, 暂不接受来自此频道的投稿";
                    keyboard = null;
                    newPost.Status = PostStatus.Rejected;
                    break;
                default:
                    _logger.LogError("未知的频道选项 {channelOption}", channelOption);
                    return;
            }

            Message msg = await botClient.SendTextMessageAsync(message.Chat.Id, postText, replyToMessageId: message.MessageId, replyMarkup: keyboard, allowSendingWithoutReply: true);

            //修改数据库实体
            newPost.OriginChatID = message.Chat.Id;
            newPost.OriginMsgID = message.MessageId;
            newPost.ActionMsgID = msg.MessageId;

            if (directPost)
            {
                newPost.ReviewMsgID = msg.MessageId;
                newPost.ManageMsgID = msg.MessageId;
            }

            await Insertable(newPost).ExecuteCommandAsync();
        }

        /// <summary>
        /// 处理多媒体投稿
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task HandleMediaPosts(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            if (!dbUser.Right.HasFlag(UserRights.SendPost))
            {
                await botClient.AutoReplyAsync("没有权限", message);
                return;
            }
            if (_channelService.ReviewGroup.Id == -1)
            {
                await botClient.AutoReplyAsync("尚未设置投稿群组, 无法接收投稿", message);
                return;
            }

            ChannelOption channelOption = ChannelOption.Normal;
            string? channelName = null, channelTitle = null;
            if (message.ForwardFromChat?.Type == ChatType.Channel)
            {
                long channelId = message.ForwardFromChat.Id;
                channelName = $"{message.ForwardFromChat.Username}/{message.ForwardFromMessageId}";
                channelTitle = message.ForwardFromChat.Title;
                channelOption = await _channelOptionService.FetchChannelOption(channelId, channelName, channelTitle);
            }

            BuildInTags tags = _textHelperService.FetchTags(message.Caption);
            string text = _textHelperService.ParseMessage(message);

            bool anonymous = dbUser.PreferAnonymous;

            //直接发布模式
            bool directPost = dbUser.Right.HasFlag(UserRights.DirectPost);
            //发送确认消息
            var keyboard = directPost ? _markupHelperService.DirectPostKeyboard(anonymous, tags) : _markupHelperService.PostKeyboard(anonymous);
            string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";

            //生成数据库实体
            Posts newPost = new()
            {
                Anonymous = anonymous,
                Text = text,
                RawText = message.Text ?? "",
                ChannelName = channelName ?? "",
                ChannelTitle = channelTitle ?? "",
                Status = directPost ? PostStatus.Reviewing : PostStatus.Padding,
                PostType = message.Type,
                Tags = tags,
                PosterUID = dbUser.UserID
            };

            //套用频道设定
            switch (channelOption)
            {
                case ChannelOption.Normal:
                    break;
                case ChannelOption.PurgeOrigin:
                    postText += "\n由于系统设定, 来自该频道的投稿将不会显示来源";
                    newPost.ChannelName += '~';
                    break;
                case ChannelOption.AutoReject:
                    postText = "由于系统设定, 暂不接受来自此频道的投稿";
                    keyboard = null;
                    newPost.Status = PostStatus.Rejected;
                    break;
                default:
                    _logger.LogError("未知的频道选项 {channelOption}", channelOption);
                    return;
            }

            Message msg = await botClient.SendTextMessageAsync(message.Chat.Id, postText, replyToMessageId: message.MessageId, replyMarkup: keyboard, allowSendingWithoutReply: true);

            //修改数据库实体
            newPost.OriginChatID = message.Chat.Id;
            newPost.OriginMsgID = message.MessageId;
            newPost.ActionMsgID = msg.MessageId;

            if (directPost)
            {
                newPost.ReviewMsgID = msg.MessageId;
                newPost.ManageMsgID = msg.MessageId;
            }

            long postID = await Insertable(newPost).ExecuteReturnBigIdentityAsync();

            Attachments? attachment = _attachmentHelperService.GenerateAttachment(message, postID);

            if (attachment != null)
            {
                await _attachmentRepository.Insertable(attachment).ExecuteCommandAsync();
            }
        }

        /// <summary>
        /// mediaGroupID字典
        /// </summary>
        private ConcurrentDictionary<string, long> MediaGroupIDs { get; } = new();

        /// <summary>
        /// 处理多媒体投稿(mediaGroup)
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task HandleMediaGroupPosts(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            if (!dbUser.Right.HasFlag(UserRights.SendPost))
            {
                await botClient.AutoReplyAsync("没有权限", message);
                return;
            }
            if (_channelService.ReviewGroup.Id == -1)
            {
                await botClient.AutoReplyAsync("尚未设置投稿群组, 无法接收投稿", message);
                return;
            }

            string mediaGroupId = message.MediaGroupId!;
            if (!MediaGroupIDs.TryGetValue(mediaGroupId, out long postID)) //如果mediaGroupId不存在则创建新Post
            {
                MediaGroupIDs.TryAdd(mediaGroupId, -1);

                bool exists = await Queryable().AnyAsync(x => x.MediaGroupID == mediaGroupId);
                if (!exists)
                {
                    ChannelOption channelOption = ChannelOption.Normal;
                    string? channelName = null, channelTitle = null;
                    if (message.ForwardFromChat?.Type == ChatType.Channel)
                    {
                        long channelId = message.ForwardFromChat.Id;
                        channelName = $"{message.ForwardFromChat.Username}/{message.ForwardFromMessageId}";
                        channelTitle = message.ForwardFromChat.Title;
                        channelOption = await _channelOptionService.FetchChannelOption(channelId, channelName, channelTitle);
                    }

                    BuildInTags tags = _textHelperService.FetchTags(message.Caption);
                    string text = _textHelperService.ParseMessage(message);

                    bool anonymous = dbUser.PreferAnonymous;

                    //直接发布模式
                    bool directPost = dbUser.Right.HasFlag(UserRights.DirectPost);

                    //发送确认消息
                    var keyboard = directPost ? _markupHelperService.DirectPostKeyboard(anonymous, tags) : _markupHelperService.PostKeyboard(anonymous);
                    string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";

                    Message msg = await botClient.SendTextMessageAsync(message.Chat.Id, "处理中, 请稍后", replyToMessageId: message.MessageId, allowSendingWithoutReply: true);

                    //生成数据库实体
                    Posts newPost = new()
                    {
                        OriginChatID = message.Chat.Id,
                        OriginMsgID = message.MessageId,
                        ActionMsgID = msg.MessageId,
                        Anonymous = anonymous,
                        Text = text,
                        RawText = message.Text ?? "",
                        ChannelName = channelName ?? "",
                        ChannelTitle = channelTitle ?? "",
                        Status = directPost ? PostStatus.Reviewing : PostStatus.Padding,
                        PostType = message.Type,
                        MediaGroupID = mediaGroupId,
                        Tags = tags,
                        PosterUID = dbUser.UserID,
                    };

                    //套用频道设定
                    switch (channelOption)
                    {
                        case ChannelOption.Normal:
                            break;
                        case ChannelOption.PurgeOrigin:
                            postText += "\n由于系统设定, 来自该频道的投稿将不会显示来源";
                            newPost.ChannelName += '~';
                            break;
                        case ChannelOption.AutoReject:
                            postText = "由于系统设定, 暂不接受来自此频道的投稿";
                            keyboard = null;
                            newPost.Status = PostStatus.Rejected;
                            break;
                        default:
                            _logger.LogError("未知的频道选项 {channelOption}", channelOption);
                            return;
                    }

                    if (directPost)
                    {
                        newPost.ReviewMsgID = msg.MessageId;
                        newPost.ManageMsgID = msg.MessageId;
                    }

                    postID = await Insertable(newPost).ExecuteReturnBigIdentityAsync();

                    MediaGroupIDs[mediaGroupId] = postID;

                    //两秒后停止接收媒体组消息
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1500);
                        MediaGroupIDs.Remove(mediaGroupId, out _);

                        await botClient.EditMessageTextAsync(msg, postText, replyMarkup: keyboard);
                    });
                }
            }

            //更新附件
            if (postID > 0)
            {
                Attachments? attachment = _attachmentHelperService.GenerateAttachment(message, postID);

                if (attachment != null)
                {
                    await _attachmentRepository.Insertable(attachment).ExecuteCommandAsync();
                }
            }
        }

        /// <summary>
        /// 设置稿件Tag
        /// </summary>
        /// <param name="post"></param>
        /// <param name="tag"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        public async Task SetPostTag(Posts post, BuildInTags tag, CallbackQuery callbackQuery)
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
            await Updateable(post).UpdateColumns(x => new { x.Tags, x.ModifyAt }).ExecuteCommandAsync();

            await _botClient.AutoReplyAsync(string.Join(' ', tagNames), callbackQuery);

            var keyboard = post.IsDirectPost ?
                _markupHelperService.DirectPostKeyboard(post.Anonymous, post.Tags) :
                _markupHelperService.ReviewKeyboardA(post.Tags);
            await _botClient.EditMessageReplyMarkupAsync(callbackQuery.Message!, keyboard);
        }

        /// <summary>
        /// 拒绝投稿
        /// </summary>
        /// <param name="post"></param>
        /// <param name="dbUser"></param>
        /// <param name="rejectReason"></param>
        /// <returns></returns>
        public async Task RejetPost(Posts post, Users dbUser, string rejectReason)
        {
            post.ReviewerUID = dbUser.UserID;
            post.Status = PostStatus.Rejected;
            post.ModifyAt = DateTime.Now;
            await Updateable(post).UpdateColumns(x => new { x.Reason, x.ReviewerUID, x.Status, x.ModifyAt }).ExecuteCommandAsync();

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
                var messages = await _botClient.SendMediaGroupAsync(_channelService.RejectChannel.Id, group);
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
        public async Task AcceptPost(Posts post, Users dbUser, CallbackQuery callbackQuery)
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

            await Updateable(post).UpdateColumns(x => new { x.ReviewMsgID, x.PublicMsgID, x.ReviewerUID, x.Status, x.ModifyAt }).ExecuteCommandAsync();

            //通知投稿人
            bool directPost = post.ManageMsgID == post.ActionMsgID;

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
