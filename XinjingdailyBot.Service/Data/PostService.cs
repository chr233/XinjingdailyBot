using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Infrastructure.Localization;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data
{
    [AppService(typeof(IPostService), LifeTime.Singleton)]
    public sealed class PostService : BaseService<Posts>, IPostService
    {
        private readonly ILogger<PostService> _logger;
        private readonly IAttachmentService _attachmentService;
        private readonly IChannelService _channelService;
        private readonly IChannelOptionService _channelOptionService;
        private readonly ITextHelperService _textHelperService;
        private readonly IMarkupHelperService _markupHelperService;
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly OptionsSetting.PostOption _postOption;
        private readonly TagRepository _tagRepository;

        public PostService(
            ILogger<PostService> logger,
            IAttachmentService attachmentService,
            IChannelService channelService,
            IChannelOptionService channelOptionService,
            ITextHelperService textHelperService,
            IMarkupHelperService markupHelperService,
            ITelegramBotClient botClient,
            IUserService userService,
            IOptions<OptionsSetting> options,
            TagRepository tagRepository)
        {
            _logger = logger;
            _attachmentService = attachmentService;
            _channelService = channelService;
            _channelOptionService = channelOptionService;
            _textHelperService = textHelperService;
            _markupHelperService = markupHelperService;
            _botClient = botClient;
            _userService = userService;
            _postOption = options.Value.Post;
            _tagRepository = tagRepository;
        }

        /// <summary>
        /// 检查用户是否达到每日投稿上限
        /// </summary>
        /// <param name="dbUser"></param>
        /// <returns>true: 可以继续投稿 false: 无法继续投稿</returns>
        public async Task<bool> CheckPostLimit(Users dbUser, Message? message = null, CallbackQuery? query = null)
        {
            // 未开启限制或者用户为管理员时不受限制
            if (!_postOption.EnablePostLimit || dbUser.Right.HasFlag(UserRights.Admin))
            {
                return true;
            }

            DateTime now = DateTime.Now;
            DateTime today = now.AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);

            //待确认
            var paddingCount = await Queryable()
                .Where(x => x.PosterUID == dbUser.UserID && x.CreateAt >= today && x.Status == PostStatus.Padding)
                .CountAsync();

            int paddingLimit = _postOption.DailyPaddingLimit;
            if (paddingCount >= paddingLimit)
            {
                if (message != null)
                {
                    await _botClient.AutoReplyAsync($"您的投稿队列已满 {paddingCount} / {paddingLimit}, 请先处理尚未确认的稿件", message);
                }
                if (query != null)
                {
                    await _botClient.AutoReplyAsync($"您的投稿队列已满 {paddingCount} / {paddingLimit}, 请先处理尚未确认的稿件", query, true);
                }
                return false;
            }

            int baseRatio = Math.Min(dbUser.AcceptCount / _postOption.RatioDivisor + 1, _postOption.MaxRatio);

            //审核中
            var reviewCount = await Queryable()
                .Where(x => x.PosterUID == dbUser.UserID && x.CreateAt >= today && x.Status == PostStatus.Reviewing)
                .CountAsync();

            int reviewLimit = baseRatio * _postOption.DailyReviewLimit;
            if (reviewCount >= reviewLimit)
            {
                if (message != null)
                {
                    await _botClient.AutoReplyAsync($"您的审核队列已满 {reviewCount} / {reviewLimit}, 请耐心等待队列中的稿件审核完毕", message);
                    return true;
                }
                if (query != null)
                {
                    await _botClient.AutoReplyAsync($"您的审核队列已满 {reviewCount} / {reviewLimit}, 请耐心等待队列中的稿件审核完毕", query, true);
                    return false;
                }
            }

            //已通过 + 已拒绝(非重复/模糊原因)
            var postCount = await Queryable()
                .Where(x => x.PosterUID == dbUser.UserID && x.CreateAt >= today &&
                    (x.Status == PostStatus.Accepted || (x.Status == PostStatus.Rejected && x.Reason != RejectReason.Duplicate && x.Reason != RejectReason.Fuzzy)))
                .CountAsync();

            int dailyLimit = baseRatio * _postOption.DailyPostLimit;
            if (postCount >= dailyLimit)
            {
                if (message != null)
                {
                    await _botClient.AutoReplyAsync($"您已达到每日投稿上限 {postCount} / {dailyLimit}, 暂时无法继续投稿, 请明日再来", message);
                    return true;
                }
                if (query != null)
                {
                    await _botClient.AutoReplyAsync($"您已达到每日投稿上限 {postCount} / {dailyLimit}, 暂时无法继续投稿, 请明日再来", query, true);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 处理文字投稿
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task HandleTextPosts(Users dbUser, Message message)
        {
            if (!dbUser.Right.HasFlag(UserRights.SendPost))
            {
                await _botClient.AutoReplyAsync(Langs.NoPostRight, message);
                return;
            }
            if (_channelService.ReviewGroup.Id == -1)
            {
                await _botClient.AutoReplyAsync(Langs.ReviewGroupNotSet, message);
                return;
            }

            if (string.IsNullOrEmpty(message.Text))
            {
                await _botClient.AutoReplyAsync(Langs.TextPostCantBeNull, message);
                return;
            }

            if (message.Text.Length > IPostService.MaxPostText)
            {
                await _botClient.AutoReplyAsync($"文本长度超过上限 {IPostService.MaxPostText}, 无法创建投稿", message);
                return;
            }

            ChannelOption channelOption = ChannelOption.Normal;
            string? channelName = null, channelTitle = null;
            if (message.ForwardFromChat?.Type == ChatType.Channel)
            {
                long channelId = message.ForwardFromChat.Id;
                //非管理员禁止从自己的频道转发
                if (!dbUser.Right.HasFlag(UserRights.ReviewPost))
                {
                    if (channelId == _channelService.AcceptChannel.Id || channelId == _channelService.RejectChannel.Id)
                    {
                        await _botClient.AutoReplyAsync("禁止从发布频道或者拒稿频道转载投稿内容", message);
                        return;
                    }
                }

                channelTitle = message.ForwardFromChat.Title;
                channelOption = await _channelOptionService.FetchChannelOption(channelId, message.ForwardFromChat.Username, channelTitle);
                channelName = $"{message.ForwardFromChat.Username}/{message.ForwardFromMessageId}";
            }

            BuildInTags tags = _textHelperService.FetchTags(message.Text);
            int newTags = _tagRepository.FetchTags(message.Text);
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

            Message msg = await _botClient.SendTextMessageAsync(message.Chat.Id, postText, replyToMessageId: message.MessageId, replyMarkup: keyboard, allowSendingWithoutReply: true);

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
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task HandleMediaPosts(Users dbUser, Message message)
        {
            if (!dbUser.Right.HasFlag(UserRights.SendPost))
            {
                await _botClient.AutoReplyAsync("没有权限", message);
                return;
            }
            if (_channelService.ReviewGroup.Id == -1)
            {
                await _botClient.AutoReplyAsync("尚未设置投稿群组, 无法接收投稿", message);
                return;
            }

            ChannelOption channelOption = ChannelOption.Normal;
            string? channelName = null, channelTitle = null;
            if (message.ForwardFromChat?.Type == ChatType.Channel)
            {
                long channelId = message.ForwardFromChat.Id;
                //非管理员禁止从自己的频道转发
                if (!dbUser.Right.HasFlag(UserRights.ReviewPost))
                {
                    if (channelId == _channelService.AcceptChannel.Id || channelId == _channelService.RejectChannel.Id)
                    {
                        await _botClient.AutoReplyAsync("禁止从发布频道或者拒稿频道转载投稿内容", message);
                        return;
                    }
                }

                channelTitle = message.ForwardFromChat.Title;
                channelOption = await _channelOptionService.FetchChannelOption(channelId, message.ForwardFromChat.Username, channelTitle);
                channelName = $"{message.ForwardFromChat.Username}/{message.ForwardFromMessageId}";
            }

            BuildInTags tags = _textHelperService.FetchTags(message.Caption);
            string text = _textHelperService.ParseMessage(message);

            if (message.HasMediaSpoiler == true)
            {
                tags |= BuildInTags.Spoiler;
            }

            bool anonymous = dbUser.PreferAnonymous;

            //直接发布模式
            bool directPost = dbUser.Right.HasFlag(UserRights.DirectPost);
            bool hasSpoiler = message.Type == MessageType.Photo || message.Type == MessageType.Video;

            //发送确认消息
            var keyboard = directPost ?
                (hasSpoiler ? _markupHelperService.DirectPostKeyboardWithSpoiler(anonymous, tags) : _markupHelperService.DirectPostKeyboard(anonymous, tags)) :
               _markupHelperService.PostKeyboard(anonymous);
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

            Message msg = await _botClient.SendTextMessageAsync(message.Chat.Id, postText, replyToMessageId: message.MessageId, replyMarkup: keyboard, allowSendingWithoutReply: true);

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

            Attachments? attachment = _attachmentService.GenerateAttachment(message, postID);

            if (attachment != null)
            {
                await _attachmentService.Insertable(attachment).ExecuteCommandAsync();
            }
        }

        /// <summary>
        /// mediaGroupID字典
        /// </summary>
        private ConcurrentDictionary<string, long> MediaGroupIDs { get; } = new();

        /// <summary>
        /// 处理多媒体投稿(mediaGroup)
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task HandleMediaGroupPosts(Users dbUser, Message message)
        {
            if (!dbUser.Right.HasFlag(UserRights.SendPost))
            {
                await _botClient.AutoReplyAsync("没有权限", message);
                return;
            }
            if (_channelService.ReviewGroup.Id == -1)
            {
                await _botClient.AutoReplyAsync("尚未设置投稿群组, 无法接收投稿", message);
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
                        //非管理员禁止从自己的频道转发
                        if (!dbUser.Right.HasFlag(UserRights.ReviewPost))
                        {
                            if (channelId == _channelService.AcceptChannel.Id || channelId == _channelService.RejectChannel.Id)
                            {
                                await _botClient.AutoReplyAsync("禁止从发布频道或者拒稿频道转载投稿内容", message);
                                return;
                            }
                        }

                        channelTitle = message.ForwardFromChat.Title;
                        channelOption = await _channelOptionService.FetchChannelOption(channelId, message.ForwardFromChat.Username, channelTitle);
                        channelName = $"{message.ForwardFromChat.Username}/{message.ForwardFromMessageId}";
                    }

                    BuildInTags tags = _textHelperService.FetchTags(message.Caption);
                    string text = _textHelperService.ParseMessage(message);

                    bool anonymous = dbUser.PreferAnonymous;

                    if (message.HasMediaSpoiler == true)
                    {
                        tags |= BuildInTags.Spoiler;
                    }

                    //直接发布模式
                    bool directPost = dbUser.Right.HasFlag(UserRights.DirectPost);
                    bool hasSpoiler = message.Type == MessageType.Photo || message.Type == MessageType.Video;

                    //发送确认消息
                    var keyboard = directPost ? (hasSpoiler ? _markupHelperService.DirectPostKeyboardWithSpoiler(anonymous, tags) :
                        _markupHelperService.DirectPostKeyboard(anonymous, tags)) : _markupHelperService.PostKeyboard(anonymous);
                    string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";

                    Message msg = await _botClient.SendTextMessageAsync(message.Chat.Id, "处理中, 请稍后", replyToMessageId: message.MessageId, allowSendingWithoutReply: true);

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

                        await _botClient.EditMessageTextAsync(msg, postText, replyMarkup: keyboard);
                    });
                }
            }

            //更新附件
            if (postID > 0)
            {
                Attachments? attachment = _attachmentService.GenerateAttachment(message, postID);

                if (attachment != null)
                {
                    await _attachmentService.Insertable(attachment).ExecuteCommandAsync();
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

            bool hasSpoiler = post.PostType == MessageType.Photo || post.PostType == MessageType.Video;

            var keyboard = hasSpoiler ?
                (post.IsDirectPost ?
                    _markupHelperService.DirectPostKeyboardWithSpoiler(post.Anonymous, post.Tags) :
                    _markupHelperService.ReviewKeyboardAWithSpoiler(post.Tags)) :
                (post.IsDirectPost ?
                    _markupHelperService.DirectPostKeyboard(post.Anonymous, post.Tags) :
                    _markupHelperService.ReviewKeyboardA(post.Tags));
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
                if (post.PostType != MessageType.Text)
                {
                    var attachment = await _attachmentService.Queryable().Where(x => x.PostID == post.Id).FirstAsync();

                    var handler = post.PostType switch
                    {
                        MessageType.Photo => _botClient.SendPhotoAsync(_channelService.RejectChannel.Id, new InputFileId(attachment.FileID)),
                        MessageType.Audio => _botClient.SendAudioAsync(_channelService.RejectChannel.Id, new InputFileId(attachment.FileID)),
                        MessageType.Video => _botClient.SendVideoAsync(_channelService.RejectChannel.Id, new InputFileId(attachment.FileID)),
                        MessageType.Voice => _botClient.SendVoiceAsync(_channelService.RejectChannel.Id, new InputFileId(attachment.FileID)),
                        MessageType.Document => _botClient.SendDocumentAsync(_channelService.RejectChannel.Id, new InputFileId(attachment.FileID)),
                        MessageType.Animation => _botClient.SendAnimationAsync(_channelService.RejectChannel.Id, new InputFileId(attachment.FileID)),
                        _ => throw new Exception("未知的稿件类型"),
                    };

                    if (handler != null)
                    {
                        await handler;
                    }
                }
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
                        MessageType.Photo => new InputMediaPhoto(new InputFileId(attachments[i].FileID)),
                        MessageType.Audio => new InputMediaAudio(new InputFileId(attachments[i].FileID)),
                        MessageType.Video => new InputMediaVideo(new InputFileId(attachments[i].FileID)),
                        MessageType.Voice => new InputMediaAudio(new InputFileId(attachments[i].FileID)),
                        MessageType.Document => new InputMediaDocument(new InputFileId(attachments[i].FileID)),
                        //MessageType.Animation 不支持媒体组
                        _ => throw new Exception("未知的稿件类型"),
                    };
                }
                var _ = await _botClient.SendMediaGroupAsync(_channelService.RejectChannel.Id, group);
                //处理媒体组消息 TODO
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

            poster.RejectCount++;
            poster.ModifyAt = DateTime.Now;
            await _userService.Updateable(poster).UpdateColumns(x => new { x.RejectCount, x.ModifyAt }).ExecuteCommandAsync();

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

            if (post.IsDirectPost)
            {
                poster.PostCount++;
            }

            string postText = _textHelperService.MakePostText(post, poster);

            bool hasSpoiler = post.Tags.HasFlag(BuildInTags.Spoiler);

            //发布频道发布消息
            if (!post.IsMediaGroup)
            {
                if (post.Tags.HasFlag(BuildInTags.NSFW))
                {
                    await _botClient.SendTextMessageAsync(_channelService.AcceptChannel.Id, _textHelperService.NSFWWrning, allowSendingWithoutReply: true);
                }

                Message? msg = null;
                if (post.PostType == MessageType.Text)
                {
                    msg = await _botClient.SendTextMessageAsync(_channelService.AcceptChannel.Id, postText, parseMode: ParseMode.Html, disableWebPagePreview: true);
                }
                else
                {
                    Attachments attachment = await _attachmentService.Queryable().FirstAsync(x => x.PostID == post.Id);

                    var handler = post.PostType switch
                    {
                        MessageType.Photo => _botClient.SendPhotoAsync(_channelService.AcceptChannel.Id, new InputFileId(attachment.FileID), caption: postText, parseMode: ParseMode.Html, hasSpoiler: hasSpoiler),
                        MessageType.Audio => _botClient.SendAudioAsync(_channelService.AcceptChannel.Id, new InputFileId(attachment.FileID), caption: postText, parseMode: ParseMode.Html, title: attachment.FileName),
                        MessageType.Video => _botClient.SendVideoAsync(_channelService.AcceptChannel.Id, new InputFileId(attachment.FileID), caption: postText, parseMode: ParseMode.Html, hasSpoiler: hasSpoiler),
                        MessageType.Voice => _botClient.SendVoiceAsync(_channelService.AcceptChannel.Id, new InputFileId(attachment.FileID), caption: postText, parseMode: ParseMode.Html),
                        MessageType.Document => _botClient.SendDocumentAsync(_channelService.AcceptChannel.Id, new InputFileId(attachment.FileID), caption: postText, parseMode: ParseMode.Html),
                        MessageType.Animation => _botClient.SendDocumentAsync(_channelService.AcceptChannel.Id, new InputFileId(attachment.FileID), caption: postText, parseMode: ParseMode.Html),
                        _ => null,
                    };

                    if (handler == null)
                    {
                        await _botClient.AutoReplyAsync($"不支持的稿件类型: {post.PostType}", callbackQuery);
                        return;
                    }

                    msg = await handler;
                }
                post.PublicMsgID = msg?.MessageId ?? -1;
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
                        MessageType.Photo => new InputMediaPhoto(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html, HasSpoiler = hasSpoiler },
                        MessageType.Audio => new InputMediaAudio(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                        MessageType.Video => new InputMediaVideo(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html, HasSpoiler = hasSpoiler },
                        MessageType.Voice => new InputMediaVideo(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                        MessageType.Document => new InputMediaDocument(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                        //MessageType.Animation 不支持媒体组
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
            string posterMsg = _textHelperService.MakeNotification(post.IsDirectPost, post.PublicMsgID);

            if (poster.Notification && poster.UserID != dbUser.UserID)//启用通知并且审核与投稿不是同一个人
            {//单独发送通知消息
                await _botClient.SendTextMessageAsync(post.OriginChatID, posterMsg, parseMode: ParseMode.Html, replyToMessageId: (int)post.OriginMsgID, allowSendingWithoutReply: true, disableWebPagePreview: true);
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
