using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
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

namespace XinjingdailyBot.Service.Data;

/// <inheritdoc cref="IPostService"/>
[AppService(typeof(IPostService), LifeTime.Singleton)]
internal sealed class PostService : BaseService<NewPosts>, IPostService
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
    private readonly IMediaGroupService _mediaGroupService;

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
        TagRepository tagRepository,
        IMediaGroupService mediaGroupService)
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
        _mediaGroupService = mediaGroupService;
    }

    public async Task<bool> CheckPostLimit(Users dbUser, Message? message = null, CallbackQuery? query = null)
    {
        //未开启限制或者用户为管理员时不受限制
        if ((dbUser.AcceptCount > 0 && !_postOption.EnablePostLimit) || dbUser.Right.HasFlag(EUserRights.Admin))
        {
            return true;
        }

        //待定确认稿件上限
        int paddingLimit = _postOption.DailyPaddingLimit;
        //上限基数
        int baseRatio = Math.Min(dbUser.AcceptCount / _postOption.RatioDivisor + 1, _postOption.MaxRatio);
        //审核中稿件上限
        int reviewLimit = baseRatio * _postOption.DailyReviewLimit;
        //每日投稿上限
        int dailyLimit = baseRatio * _postOption.DailyPostLimit;

        //没有通过稿件的用户收到更严格的限制
        if (dbUser.AcceptCount == 0)
        {
            paddingLimit = 2;
            reviewLimit = 1;
            dailyLimit = 1;
        }

        var now = DateTime.Now;
        var today = now.AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);

        if (message != null)
        {
            //待确认
            int paddingCount = await Queryable()
                .Where(x => x.PosterUID == dbUser.UserID && x.CreateAt >= today && x.Status == EPostStatus.Padding)
                .CountAsync();

            if (paddingCount >= paddingLimit)
            {
                await _botClient.AutoReplyAsync($"您的投稿队列已满 {paddingCount} / {paddingLimit}, 请先处理尚未确认的稿件", message);
                return false;
            }

            //已通过 + 已拒绝(非重复 / 模糊原因)
            int postCount = await Queryable()
                .Where(x => x.PosterUID == dbUser.UserID && x.CreateAt >= today && (x.Status == EPostStatus.Accepted || (x.Status == EPostStatus.Rejected && x.CountReject)))
                .CountAsync();

            if (postCount >= dailyLimit)
            {
                await _botClient.AutoReplyAsync($"您已达到每日投稿上限 {postCount} / {dailyLimit}, 暂时无法继续投稿, 请明日再来", message);
                return true;
            }
        }

        if (query != null)
        {
            //审核中
            int reviewCount = await Queryable()
                .Where(x => x.PosterUID == dbUser.UserID && x.CreateAt >= today && x.Status == EPostStatus.Reviewing)
                .CountAsync();

            if (reviewCount >= reviewLimit)
            {
                await _botClient.AutoReplyAsync($"您的审核队列已满 {reviewCount} / {reviewLimit}, 请耐心等待队列中的稿件审核完毕", query, true);
                return false;
            }
        }

        return true;
    }

    public async Task HandleTextPosts(Users dbUser, Message message)
    {
        if (!dbUser.Right.HasFlag(EUserRights.SendPost))
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

        var channelOption = EChannelOption.Normal;

        long channelId = -1, channelMsgId = -1;
        if (message.ForwardFromChat?.Type == ChatType.Channel)
        {
            channelId = message.ForwardFromChat.Id;
            //非管理员禁止从自己的频道转发
            if (!dbUser.Right.HasFlag(EUserRights.ReviewPost))
            {
                if (channelId == _channelService.AcceptChannel.Id || channelId == _channelService.RejectChannel.Id)
                {
                    await _botClient.AutoReplyAsync("禁止从发布频道或者拒稿频道转载投稿内容", message);
                    return;
                }
            }

            channelMsgId = message.ForwardFromMessageId ?? -1;
            channelOption = await _channelOptionService.FetchChannelOption(message.ForwardFromChat);
        }

        int newTags = _tagRepository.FetchTags(message.Text);
        string text = _textHelperService.ParseMessage(message);

        bool anonymous = dbUser.PreferAnonymous;

        //直接发布模式
        bool directPost = dbUser.Right.HasFlag(EUserRights.DirectPost);

        //发送确认消息
        var keyboard = directPost ? _markupHelperService.DirectPostKeyboard(anonymous, newTags, null) : _markupHelperService.PostKeyboard(anonymous);
        string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";

        //生成数据库实体
        var newPost = new NewPosts {
            Anonymous = anonymous,
            Text = text,
            RawText = message.Text ?? "",
            ChannelID = channelId,
            ChannelMsgID = channelMsgId,
            Status = directPost ? EPostStatus.Reviewing : EPostStatus.Padding,
            PostType = message.Type,
            Tags = newTags,
            HasSpoiler = message.HasMediaSpoiler ?? false,
            PosterUID = dbUser.UserID
        };

        //套用频道设定
        switch (channelOption)
        {
            case EChannelOption.Normal:
                break;
            case EChannelOption.PurgeOrigin:
                postText += "\n由于系统设定, 来自该频道的投稿将不会显示来源";
                break;
            case EChannelOption.AutoReject:
                postText = "由于系统设定, 暂不接受来自此频道的投稿";
                keyboard = null;
                newPost.Status = EPostStatus.Rejected;
                break;
            default:
                _logger.LogError("未知的频道选项 {channelOption}", channelOption);
                return;
        }

        var actionMsg = await _botClient.SendTextMessageAsync(message.Chat.Id, postText, replyToMessageId: message.MessageId, replyMarkup: keyboard, allowSendingWithoutReply: true);

        //修改数据库实体
        newPost.OriginChatID = message.Chat.Id;
        newPost.OriginMsgID = message.MessageId;
        newPost.OriginActionChatID = actionMsg.Chat.Id;
        newPost.OriginActionMsgID = actionMsg.MessageId;

        if (directPost)
        {
            newPost.ReviewChatID = newPost.OriginChatID;
            newPost.ReviewMsgID = newPost.OriginMsgID;
            newPost.ReviewActionChatID = newPost.OriginActionChatID;
            newPost.ReviewActionMsgID = newPost.OriginActionMsgID;
        }

        await Insertable(newPost).ExecuteCommandAsync();
    }

    public async Task HandleMediaPosts(Users dbUser, Message message)
    {
        if (!dbUser.Right.HasFlag(EUserRights.SendPost))
        {
            await _botClient.AutoReplyAsync("没有权限", message);
            return;
        }
        if (_channelService.ReviewGroup.Id == -1)
        {
            await _botClient.AutoReplyAsync("尚未设置投稿群组, 无法接收投稿", message);
            return;
        }

        var channelOption = EChannelOption.Normal;

        long channelId = -1, channelMsgId = -1;
        if (message.ForwardFromChat?.Type == ChatType.Channel)
        {
            channelId = message.ForwardFromChat.Id;
            //非管理员禁止从自己的频道转发
            if (!dbUser.Right.HasFlag(EUserRights.ReviewPost))
            {
                if (channelId == _channelService.AcceptChannel.Id || channelId == _channelService.RejectChannel.Id)
                {
                    await _botClient.AutoReplyAsync("禁止从发布频道或者拒稿频道转载投稿内容", message);
                    return;
                }
            }
            channelMsgId = message.ForwardFromMessageId ?? -1;
            channelOption = await _channelOptionService.FetchChannelOption(message.ForwardFromChat);
        }

        int newTags = _tagRepository.FetchTags(message.Caption);
        string text = _textHelperService.ParseMessage(message);

        bool anonymous = dbUser.PreferAnonymous;

        //直接发布模式
        bool directPost = dbUser.Right.HasFlag(EUserRights.DirectPost);

        bool? hasSpoiler = message.CanSpoiler() ? message.HasMediaSpoiler ?? false : null;

        //发送确认消息
        var keyboard = directPost ?
            _markupHelperService.DirectPostKeyboard(anonymous, newTags, hasSpoiler) :
            _markupHelperService.PostKeyboard(anonymous);
        string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";

        //生成数据库实体
        var newPost = new NewPosts {
            Anonymous = anonymous,
            Text = text,
            RawText = message.Text ?? "",
            ChannelID = channelId,
            ChannelMsgID = channelMsgId,
            Status = directPost ? EPostStatus.Reviewing : EPostStatus.Padding,
            PostType = message.Type,
            Tags = newTags,
            HasSpoiler = message.HasMediaSpoiler ?? false,
            PosterUID = dbUser.UserID
        };

        //套用频道设定
        switch (channelOption)
        {
            case EChannelOption.Normal:
                break;
            case EChannelOption.PurgeOrigin:
                postText += "\n由于系统设定, 来自该频道的投稿将不会显示来源";
                break;
            case EChannelOption.AutoReject:
                postText = "由于系统设定, 暂不接受来自此频道的投稿";
                keyboard = null;
                newPost.Status = EPostStatus.Rejected;
                break;
            default:
                _logger.LogError("未知的频道选项 {channelOption}", channelOption);
                return;
        }

        var actionMsg = await _botClient.SendTextMessageAsync(message.Chat.Id, postText, replyToMessageId: message.MessageId, replyMarkup: keyboard, allowSendingWithoutReply: true);

        //修改数据库实体
        newPost.OriginChatID = message.Chat.Id;
        newPost.OriginMsgID = message.MessageId;
        newPost.OriginActionChatID = actionMsg.Chat.Id;
        newPost.OriginActionMsgID = actionMsg.MessageId;

        if (directPost)
        {
            newPost.ReviewChatID = newPost.OriginChatID;
            newPost.ReviewMsgID = newPost.OriginMsgID;
            newPost.ReviewActionChatID = newPost.OriginActionChatID;
            newPost.ReviewActionMsgID = newPost.OriginActionMsgID;
        }

        long postID = await Insertable(newPost).ExecuteReturnBigIdentityAsync();

        var attachment = _attachmentService.GenerateAttachment(message, postID);

        if (attachment != null)
        {
            await _attachmentService.Insertable(attachment).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// mediaGroupID字典
    /// </summary>
    private ConcurrentDictionary<string, int> MediaGroupIDs { get; } = new();

    public async Task HandleMediaGroupPosts(Users dbUser, Message message)
    {
        if (!dbUser.Right.HasFlag(EUserRights.SendPost))
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
        if (!MediaGroupIDs.TryGetValue(mediaGroupId, out int postID)) //如果mediaGroupId不存在则创建新Post
        {
            MediaGroupIDs.TryAdd(mediaGroupId, -1);

            bool exists = await Queryable().AnyAsync(x => x.OriginMediaGroupID == mediaGroupId);
            if (!exists)
            {
                var channelOption = EChannelOption.Normal;

                long channelId = -1, channelMsgId = -1;
                if (message.ForwardFromChat?.Type == ChatType.Channel)
                {
                    channelId = message.ForwardFromChat.Id;
                    //非管理员禁止从自己的频道转发
                    if (!dbUser.Right.HasFlag(EUserRights.ReviewPost))
                    {
                        if (channelId == _channelService.AcceptChannel.Id || channelId == _channelService.RejectChannel.Id)
                        {
                            await _botClient.AutoReplyAsync("禁止从发布频道或者拒稿频道转载投稿内容", message);
                            return;
                        }
                    }

                    channelMsgId = message.ForwardFromMessageId ?? -1;
                    channelOption = await _channelOptionService.FetchChannelOption(message.ForwardFromChat);
                }

                int newTags = _tagRepository.FetchTags(message.Caption);
                string text = _textHelperService.ParseMessage(message);

                bool anonymous = dbUser.PreferAnonymous;

                //直接发布模式
                bool directPost = dbUser.Right.HasFlag(EUserRights.DirectPost);
                bool? hasSpoiler = message.CanSpoiler() ? message.HasMediaSpoiler ?? false : null;

                //发送确认消息
                var keyboard = directPost ?
                    _markupHelperService.DirectPostKeyboard(anonymous, newTags, hasSpoiler) :
                    _markupHelperService.PostKeyboard(anonymous);
                string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";

                var actionMsg = await _botClient.SendTextMessageAsync(message.Chat.Id, "处理中, 请稍后", replyToMessageId: message.MessageId, allowSendingWithoutReply: true);

                //生成数据库实体
                var newPost = new NewPosts {
                    OriginChatID = message.Chat.Id,
                    OriginMsgID = message.MessageId,
                    OriginActionChatID = actionMsg.Chat.Id,
                    OriginActionMsgID = actionMsg.MessageId,
                    Anonymous = anonymous,
                    Text = text,
                    RawText = message.Text ?? "",
                    ChannelID = channelId,
                    ChannelMsgID = channelMsgId,
                    Status = directPost ? EPostStatus.Reviewing : EPostStatus.Padding,
                    PostType = message.Type,
                    OriginMediaGroupID = mediaGroupId,
                    Tags = newTags,
                    HasSpoiler = hasSpoiler ?? false,
                    PosterUID = dbUser.UserID,
                };

                //套用频道设定
                switch (channelOption)
                {
                    case EChannelOption.Normal:
                        break;
                    case EChannelOption.PurgeOrigin:
                        postText += "\n由于系统设定, 来自该频道的投稿将不会显示来源";
                        break;
                    case EChannelOption.AutoReject:
                        postText = "由于系统设定, 暂不接受来自此频道的投稿";
                        keyboard = null;
                        newPost.Status = EPostStatus.Rejected;
                        break;
                    default:
                        _logger.LogError("未知的频道选项 {channelOption}", channelOption);
                        return;
                }

                if (directPost)
                {
                    newPost.ReviewChatID = newPost.OriginChatID;
                    newPost.ReviewMsgID = newPost.OriginMsgID;
                    newPost.ReviewActionChatID = newPost.OriginActionChatID;
                    newPost.ReviewActionMsgID = newPost.OriginActionMsgID;
                }

                postID = await Insertable(newPost).ExecuteReturnIdentityAsync();

                MediaGroupIDs[mediaGroupId] = postID;

                //两秒后停止接收媒体组消息
                _ = Task.Run(async () => {
                    await Task.Delay(1500);
                    MediaGroupIDs.Remove(mediaGroupId, out _);

                    await _botClient.EditMessageTextAsync(actionMsg, postText, replyMarkup: keyboard);
                });
            }
        }

        //更新附件
        if (postID > 0)
        {
            var attachment = _attachmentService.GenerateAttachment(message, postID);

            if (attachment != null)
            {
                await _attachmentService.Insertable(attachment).ExecuteCommandAsync();
            }
        }
    }

    public async Task SetPostTag(NewPosts post, int tagId, CallbackQuery callbackQuery)
    {
        var tag = _tagRepository.GetTagById(tagId);
        if (tag == null)
        {
            return;
        }

        if ((post.Tags & tag.Seg) > 0)
        {
            post.Tags &= ~tag.Seg;
        }
        else
        {
            post.Tags |= tag.Seg;
        }

        string tagName = _tagRepository.GetActiviedTagsName(post.Tags);

        post.ModifyAt = DateTime.Now;
        await Updateable(post).UpdateColumns(static x => new { x.Tags, x.ModifyAt }).ExecuteCommandAsync();

        await _botClient.AutoReplyAsync($"当前标签: {tagName}", callbackQuery);

        bool? hasSpoiler = post.CanSpoiler ? post.HasSpoiler : null;

        var keyboard = post.IsDirectPost ?
            _markupHelperService.DirectPostKeyboard(post.Anonymous, post.Tags, hasSpoiler) :
            _markupHelperService.ReviewKeyboardA(post.Tags, hasSpoiler);
        await _botClient.EditMessageReplyMarkupAsync(callbackQuery.Message!, keyboard);
    }

    public async Task SetPostTag(NewPosts post, string payload, CallbackQuery callbackQuery)
    {
        payload = payload.ToLowerInvariant();
        var tag = _tagRepository.GetTagByPayload(payload);
        if (tag != null)
        {
            await SetPostTag(post, tag.Id, callbackQuery);
        }
    }

    public async Task RejetPost(NewPosts post, Users dbUser, RejectReasons rejectReason)
    {
        post.RejectReason = rejectReason.Name;
        post.CountReject = rejectReason.IsCount;
        post.ReviewerUID = dbUser.UserID;
        post.Status = EPostStatus.Rejected;
        post.ModifyAt = DateTime.Now;
        await Updateable(post).UpdateColumns(static x => new {
            x.RejectReason,
            x.CountReject,
            x.ReviewerUID,
            x.Status,
            x.ModifyAt
        }).ExecuteCommandAsync();

        var poster = await _userService.Queryable().FirstAsync(x => x.UserID == post.PosterUID);

        //修改审核群消息
        string reviewMsg = _textHelperService.MakeReviewMessage(poster, dbUser, post.Anonymous, rejectReason.FullText);
        await _botClient.EditMessageTextAsync(post.ReviewActionChatID, (int)post.ReviewActionMsgID, reviewMsg, parseMode: ParseMode.Html, disableWebPagePreview: true);

        //拒稿频道发布消息
        if (!post.IsMediaGroup)
        {
            if (post.PostType != MessageType.Text)
            {
                var attachment = await _attachmentService.Queryable().Where(x => x.PostID == post.Id).FirstAsync();

                var handler = post.PostType switch {
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
                var attachmentType = attachments[i].Type;
                if (attachmentType == MessageType.Unknown)
                {
                    attachmentType = post.PostType;
                }
                group[i] = attachmentType switch {
                    MessageType.Photo => new InputMediaPhoto(new InputFileId(attachments[i].FileID)),
                    MessageType.Audio => new InputMediaAudio(new InputFileId(attachments[i].FileID)),
                    MessageType.Video => new InputMediaVideo(new InputFileId(attachments[i].FileID)),
                    MessageType.Voice => new InputMediaAudio(new InputFileId(attachments[i].FileID)),
                    MessageType.Document => new InputMediaDocument(new InputFileId(attachments[i].FileID)),
                    _ => throw new Exception("未知的稿件类型"),
                };
            }
            var postMessages = await _botClient.SendMediaGroupAsync(_channelService.RejectChannel.Id, group);

            var postMessage = postMessages.FirstOrDefault();
            if (postMessage != null)
            {
                post.PublicMsgID = postMessage.MessageId;
                post.PublishMediaGroupID = postMessage.MediaGroupId ?? "";
                post.ModifyAt = DateTime.Now;

                await Updateable(post).UpdateColumns(static x => new {
                    x.PublicMsgID,
                    x.PublishMediaGroupID,
                    x.ModifyAt
                }).ExecuteCommandAsync();
            }

            //处理媒体组消息
            await _mediaGroupService.AddPostMediaGroup(postMessages);
        }

        //通知投稿人
        string posterMsg = _textHelperService.MakeNotification(rejectReason.FullText);
        if (poster.Notification)
        {
            await _botClient.SendTextMessageAsync(post.OriginChatID, posterMsg, replyToMessageId: (int)post.OriginMsgID, allowSendingWithoutReply: true);
        }
        else
        {
            await _botClient.EditMessageTextAsync(post.OriginActionChatID, (int)post.OriginActionMsgID, posterMsg);
        }

        poster.RejectCount++;
        poster.ModifyAt = DateTime.Now;
        await _userService.Updateable(poster).UpdateColumns(static x => new { x.RejectCount, x.ModifyAt }).ExecuteCommandAsync();

        if (poster.UserID != dbUser.UserID) //非同一个人才增加审核数量
        {
            dbUser.ReviewCount++;
            dbUser.ModifyAt = DateTime.Now;
            await _userService.Updateable(dbUser).UpdateColumns(static x => new { x.ReviewCount, x.ModifyAt }).ExecuteCommandAsync();
        }
    }

    public async Task AcceptPost(NewPosts post, Users dbUser, bool inPlan, CallbackQuery callbackQuery)
    {
        var poster = await _userService.Queryable().FirstAsync(x => x.UserID == post.PosterUID);

        if (post.IsDirectPost)
        {
            poster.PostCount++;
        }

        ChannelOptions? channel = null;
        if (post.IsFromChannel)
        {
            channel = await _channelOptionService.FetchChannelByChannelId(post.ChannelID);
        }
        string postText = _textHelperService.MakePostText(post, poster, channel);

        bool hasSpoiler = post.HasSpoiler;

        if (!inPlan)
        {
            //发布频道发布消息
            if (!post.IsMediaGroup)
            {
                string? warnText = _tagRepository.GetActivedTagWarnings(post.Tags);
                if (!string.IsNullOrEmpty(warnText))
                {
                    await _botClient.SendTextMessageAsync(_channelService.AcceptChannel.Id, warnText, allowSendingWithoutReply: true);
                }

                Message? postMessage = null;
                if (post.PostType == MessageType.Text)
                {
                    postMessage = await _botClient.SendTextMessageAsync(_channelService.AcceptChannel.Id, postText, parseMode: ParseMode.Html, disableWebPagePreview: true);
                }
                else
                {
                    var attachment = await _attachmentService.Queryable().FirstAsync(x => x.PostID == post.Id);

                    var handler = post.PostType switch {
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

                    postMessage = await handler;
                }
                post.PublicMsgID = postMessage?.MessageId ?? -1;
            }
            else
            {
                var attachments = await _attachmentService.Queryable().Where(x => x.PostID == post.Id).ToListAsync();
                var group = new IAlbumInputMedia[attachments.Count];
                for (int i = 0; i < attachments.Count; i++)
                {
                    var attachmentType = attachments[i].Type;
                    if (attachmentType == MessageType.Unknown)
                    {
                        attachmentType = post.PostType;
                    }
                    group[i] = attachmentType switch {
                        MessageType.Photo => new InputMediaPhoto(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html, HasSpoiler = hasSpoiler },
                        MessageType.Audio => new InputMediaAudio(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                        MessageType.Video => new InputMediaVideo(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html, HasSpoiler = hasSpoiler },
                        MessageType.Voice => new InputMediaVideo(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                        MessageType.Document => new InputMediaDocument(new InputFileId(attachments[i].FileID)) { Caption = i == attachments.Count - 1 ? postText : null, ParseMode = ParseMode.Html },
                        _ => throw new Exception(),
                    };
                }

                string? warnText = _tagRepository.GetActivedTagWarnings(post.Tags);
                if (!string.IsNullOrEmpty(warnText))
                {
                    await _botClient.SendTextMessageAsync(_channelService.AcceptChannel.Id, warnText, allowSendingWithoutReply: true);
                }

                var postMessages = await _botClient.SendMediaGroupAsync(_channelService.AcceptChannel.Id, group);
                post.PublicMsgID = postMessages.First().MessageId;
                post.PublishMediaGroupID = postMessages.First().MediaGroupId ?? "";

                //记录媒体组消息
                await _mediaGroupService.AddPostMediaGroup(postMessages);
            }

            await _botClient.AutoReplyAsync("稿件已发布", callbackQuery);
            post.Status = EPostStatus.Accepted;
        }
        else
        {
            await _botClient.AutoReplyAsync("稿件将按设定频率定期发布", callbackQuery);
            post.Status = EPostStatus.InPlan;
        }

        post.ReviewerUID = dbUser.UserID;
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

        await Updateable(post).UpdateColumns(static x => new {
            x.ReviewMsgID,
            x.PublicMsgID,
            x.PublishMediaGroupID,
            x.ReviewerUID,
            x.Status,
            x.ModifyAt
        }).ExecuteCommandAsync();

        //通知投稿人
        string posterMsg = _textHelperService.MakeNotification(post.IsDirectPost, inPlan, post.PublicMsgID);

        if (poster.Notification && poster.UserID != dbUser.UserID)//启用通知并且审核与投稿不是同一个人
        {
            //单独发送通知消息
            await _botClient.SendTextMessageAsync(post.OriginChatID, posterMsg, parseMode: ParseMode.Html, replyToMessageId: (int)post.OriginMsgID, allowSendingWithoutReply: true, disableWebPagePreview: true);
        }
        else
        {
            //静默模式, 不单独发送通知消息
            await _botClient.EditMessageTextAsync(post.OriginChatID, (int)post.OriginActionMsgID, posterMsg, ParseMode.Html, disableWebPagePreview: true);
        }

        //增加通过数量
        poster.AcceptCount++;
        poster.ModifyAt = DateTime.Now;
        await _userService.Updateable(poster).UpdateColumns(static x => new { x.AcceptCount, x.ModifyAt }).ExecuteCommandAsync();

        if (!post.IsDirectPost) //增加审核数量
        {
            if (poster.UserID != dbUser.UserID)
            {
                dbUser.ReviewCount++;
                dbUser.ModifyAt = DateTime.Now;
                await _userService.Updateable(dbUser).UpdateColumns(static x => new { x.ReviewCount, x.ModifyAt }).ExecuteCommandAsync();
            }
        }
        else
        {
            poster.PostCount++;
            poster.ModifyAt = DateTime.Now;
            await _userService.Updateable(poster).UpdateColumns(static x => new { x.PostCount, x.ModifyAt }).ExecuteCommandAsync();
        }
    }

    public async Task<NewPosts?> FetchPostFromReplyToMessage(Message message)
    {
        var replyMessage = message.ReplyToMessage;
        if (replyMessage == null)
        {
            return null;
        }

        NewPosts? post;

        var msgGroupId = message.MediaGroupId;
        if (string.IsNullOrEmpty(msgGroupId))
        {
            //单条稿件
            long chatId = replyMessage.Chat.Id;
            int msgId = replyMessage.MessageId;
            post = await Queryable().FirstAsync(x =>
              (x.OriginChatID == chatId && x.OriginMsgID == msgId) || (x.OriginActionChatID == chatId && x.OriginActionMsgID == msgId) ||
              (x.ReviewChatID == chatId && x.ReviewMsgID == msgId) || (x.ReviewActionChatID == chatId && x.ReviewActionMsgID == msgId)
            );
        }
        else
        {
            post = await Queryable().FirstAsync(x => x.OriginMediaGroupID == msgGroupId || x.ReviewMediaGroupID == msgGroupId);
        }

        return post;
    }

    public async Task<NewPosts?> FetchPostFromCallbackQuery(CallbackQuery query)
    {
        if (query.Message == null)
        {
            return null;
        }
        var post = await FetchPostFromReplyToMessage(query.Message);
        return post;
    }
}
