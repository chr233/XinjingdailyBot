using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
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
using XinjingdailyBot.Service.Helper;

namespace XinjingdailyBot.Service.Data;

/// <inheritdoc cref="IPostService"/>
[AppService(typeof(IPostService), LifeTime.Singleton)]
internal sealed class PostService(
    ILogger<PostService> _logger,
    IAttachmentService _attachmentService,
    IChannelService _channelService,
    IChannelOptionService _channelOptionService,
    ITextHelperService _textHelperService,
    IMarkupHelperService _markupHelperService,
    ITelegramBotClient _botClient,
    IUserService _userService,
    IOptions<OptionsSetting> _options,
    TagRepository _tagRepository,
    IMediaGroupService _mediaGroupService,
    IImageHelperService _imageHelperService,
    ISqlSugarClient _context) : BaseService<NewPosts>(_context), IPostService
{
    /// <inheritdoc/>
    public async Task<bool> CheckPostLimit(Users dbUser, Message? message = null, CallbackQuery? query = null)
    {
        var postOption = _options.Value.Post;

        //未开启限制或者用户为管理员时不受限制
        if ((dbUser.AcceptCount > 0 && !postOption.EnablePostLimit) || dbUser.Right.HasFlag(EUserRights.Admin))
        {
            return true;
        }

        //待定确认稿件上限
        int paddingLimit = postOption.DailyPaddingLimit;
        //上限基数
        int baseRatio = Math.Min(dbUser.AcceptCount / postOption.RatioDivisor + 1, postOption.MaxRatio);
        //审核中稿件上限
        int reviewLimit = baseRatio * postOption.DailyReviewLimit;
        //每日投稿上限
        int dailyLimit = baseRatio * postOption.DailyPostLimit;

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
                .CountAsync().ConfigureAwait(false);

            if (paddingCount >= paddingLimit)
            {
                await _botClient.AutoReply($"您的投稿队列已满 {paddingCount} / {paddingLimit}, 请先处理尚未确认的稿件", message).ConfigureAwait(false);
                return false;
            }

            //已通过 + 已拒绝(非重复 / 模糊原因)
            int postCount = await Queryable()
                .Where(x => x.PosterUID == dbUser.UserID && x.CreateAt >= today && (x.Status == EPostStatus.Accepted || (x.Status == EPostStatus.Rejected && x.CountReject)))
                .CountAsync().ConfigureAwait(false);

            if (postCount >= dailyLimit)
            {
                await _botClient.AutoReply($"您已达到每日投稿上限 {postCount} / {dailyLimit}, 暂时无法继续投稿, 请明日再来", message).ConfigureAwait(false);
                return false;
            }
        }

        if (query != null)
        {
            //审核中
            int reviewCount = await Queryable()
                .Where(x => x.PosterUID == dbUser.UserID && x.CreateAt >= today && x.Status == EPostStatus.Reviewing)
                .CountAsync().ConfigureAwait(false);

            if (reviewCount >= reviewLimit)
            {
                await _botClient.AutoReply($"您的审核队列已满 {reviewCount} / {reviewLimit}, 请耐心等待队列中的稿件审核完毕", query, true).ConfigureAwait(false);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 纯链接解析机器人字典
    /// </summary>
    private readonly Dictionary<string, string> LinkParserBotDict = new() {
        { "b23.tv", "@bilifeedbot" },
        { "bilibili.cn", "@bilifeedbot" },
        { "bilibili.com", "@bilifeedbot" },
        { "twitter.com", "@TwPicBot" },
        { "x.com", "@TwPicBot" },
        { "fxtwitter.com", "@TwPicBot" },
        { "fixupx.com", "@TwPicBot" },
        { "fixvx.com", "@TwPicBot" },
        { "twittpr.com","@TwPicBot" },
        { "weibo.com", "@web2album_bot" },
        { "xiaohongshu.com", "@web2album_bot" },
        { "douyin.com", "@icbcbot" },
        { "youtube.com", "@icbcbot" },
        { "youtu.be", "@icbcbot" },
        { "pixiv.net", "@Pixiv_bot" },
        { "pximg.net", "@Pixiv_bot" },
    };

    /// <summary>
    /// 判断是否为纯链接
    /// </summary>
    /// <param name="msgText"></param>
    /// <returns></returns>
    private string? CheckIfRawLink(string msgText)
    {
        if (_options.Value.Bot.WarnRawLinkPost)
        {
            var matches = RegexUtils.MatchUrlHost().Matches(msgText).ToList();
            foreach (var match in matches)
            {
                string? urlHost = match.Groups[1].Value.ToLowerInvariant();
                foreach (var (host, botName) in LinkParserBotDict)
                {
                    if (urlHost.EndsWith(host))
                    {
                        return $"检测到来自 {host} 的纯链接投稿，建议先将链接发送至 {botName} 进行处理后再投稿";
                    }
                }
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task HandleTextPosts(Users dbUser, Message message)
    {
        if (!dbUser.Right.HasFlag(EUserRights.SendPost))
        {
            await _botClient.AutoReply(Langs.NoPostRight, message).ConfigureAwait(false);
            return;
        }
        if (_channelService.ReviewGroup.Id == -1)
        {
            await _botClient.AutoReply(Langs.ReviewGroupNotSet, message).ConfigureAwait(false);
            return;
        }

        if (string.IsNullOrEmpty(message.Text))
        {
            await _botClient.AutoReply(Langs.TextPostCantBeNull, message).ConfigureAwait(false);
            return;
        }

        if (message.Text.Length > IPostService.MaxPostText)
        {
            await _botClient.AutoReply($"文本长度超过上限 {IPostService.MaxPostText}, 无法创建投稿", message).ConfigureAwait(false);
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
                    await _botClient.AutoReply("禁止从发布频道或者拒稿频道转载投稿内容", message).ConfigureAwait(false);
                    return;
                }
            }

            channelMsgId = message.ForwardFromMessageId ?? -1;
            channelOption = await _channelOptionService.FetchChannelOption(message.ForwardFromChat).ConfigureAwait(false);
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
            HasSpoiler = message.HasMediaSpoiler,
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

        var actionMsg = await _botClient.SendMessageEx(message, postText, replyMarkup: keyboard).ConfigureAwait(false);

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

        await Insertable(newPost).ExecuteCommandAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task HandleMediaPosts(Users dbUser, Message message)
    {
        if (!dbUser.Right.HasFlag(EUserRights.SendPost))
        {
            await _botClient.AutoReply("没有权限", message).ConfigureAwait(false);
            return;
        }
        if (_channelService.ReviewGroup.Id == -1)
        {
            await _botClient.AutoReply("尚未设置投稿群组, 无法接收投稿", message).ConfigureAwait(false);
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
                    await _botClient.AutoReply("禁止从发布频道或者拒稿频道转载投稿内容", message).ConfigureAwait(false);
                    return;
                }
            }
            channelMsgId = message.ForwardFromMessageId ?? -1;
            channelOption = await _channelOptionService.FetchChannelOption(message.ForwardFromChat).ConfigureAwait(false);
        }

        int newTags = _tagRepository.FetchTags(message.Caption);
        string text = _textHelperService.ParseMessage(message);

        bool anonymous = dbUser.PreferAnonymous;

        //直接发布模式
        bool directPost = dbUser.Right.HasFlag(EUserRights.DirectPost);

        bool? hasSpoiler = message.CanSpoiler() ? message.HasMediaSpoiler : null;

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
            HasSpoiler = message.HasMediaSpoiler,
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

        var actionMsg = await _botClient.SendMessageEx(message, postText, replyMarkup: keyboard).ConfigureAwait(false);

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

        long postID = await Insertable(newPost).ExecuteReturnBigIdentityAsync().ConfigureAwait(false);

        await _attachmentService.CreateAttachment(message, postID).ConfigureAwait(false);
    }

    /// <summary>
    /// mediaGroupID字典
    /// </summary>
    private ConcurrentDictionary<string, int> MediaGroupIDs { get; } = new();

    /// <inheritdoc/>
    public async Task HandleMediaGroupPosts(Users dbUser, Message message)
    {
        if (!dbUser.Right.HasFlag(EUserRights.SendPost))
        {
            await _botClient.AutoReply("没有权限", message).ConfigureAwait(false);
            return;
        }
        if (_channelService.ReviewGroup.Id == -1)
        {
            await _botClient.AutoReply("尚未设置投稿群组, 无法接收投稿", message).ConfigureAwait(false);
            return;
        }

        string mediaGroupId = message.MediaGroupId!;
        if (!MediaGroupIDs.TryGetValue(mediaGroupId, out int postID)) //如果mediaGroupId不存在则创建新Post
        {
            MediaGroupIDs.TryAdd(mediaGroupId, -1);

            bool exists = await Queryable().AnyAsync(x => x.OriginMediaGroupID == mediaGroupId).ConfigureAwait(false);
            if (!exists)
            {
                await _botClient.SendChatAction(message, ChatAction.Typing).ConfigureAwait(false);

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
                            await _botClient.AutoReply("禁止从发布频道或者拒稿频道转载投稿内容", message).ConfigureAwait(false);
                            return;
                        }
                    }

                    channelMsgId = message.ForwardFromMessageId ?? -1;
                    channelOption = await _channelOptionService.FetchChannelOption(message.ForwardFromChat).ConfigureAwait(false);
                }

                int newTags = _tagRepository.FetchTags(message.Caption);
                string text = _textHelperService.ParseMessage(message);

                bool anonymous = dbUser.PreferAnonymous;

                //直接发布模式
                bool directPost = dbUser.Right.HasFlag(EUserRights.DirectPost);
                bool? hasSpoiler = message.CanSpoiler() ? message.HasMediaSpoiler : null;

                //发送确认消息
                mgCache.Keyboard = directPost ?
                    _markupHelperService.DirectPostKeyboard(anonymous, newTags, hasSpoiler) :
                    _markupHelperService.PostKeyboard(anonymous);
                string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";

                var actionMsg = await _botClient.SendMessageEx(message, "处理中, 请稍后").ConfigureAwait(false);

                //生成数据库实体
                var newPost = new NewPosts {
                    OriginChatID = message.Chat.Id,
                    OriginMsgID = message.MessageId,
                    OriginActionChatID = mgCache.ActionMessage.Chat.Id,
                    OriginActionMsgID = mgCache.ActionMessage.MessageId,
                    Anonymous = anonymous,
                    Text = text,
                    RawText = message.Text ?? "",
                    ChannelID = channelId,
                    ChannelMsgID = channelMsgId,
                    Status = channelOption == EChannelOption.AutoReject ?
                        EPostStatus.Rejected :
                        (directPost ? EPostStatus.Reviewing : EPostStatus.Padding),
                    PostType = message.Type,
                    OriginMediaGroupID = mediaGroupId,
                    Tags = newTags,
                    HasSpoiler = hasSpoiler ?? false,
                    PosterUID = dbUser.UserID,
                };

                if (directPost)
                {
                    newPost.ReviewChatID = newPost.OriginChatID;
                    newPost.ReviewMsgID = newPost.OriginMsgID;
                    newPost.ReviewActionChatID = newPost.OriginActionChatID;
                    newPost.ReviewActionMsgID = newPost.OriginActionMsgID;
                    newPost.ReviewMediaGroupID = mediaGroupId;
                }

                postID = await Insertable(newPost).ExecuteReturnIdentityAsync().ConfigureAwait(false);

                MediaGroupIDs[mediaGroupId] = postID;

                //两秒后停止接收媒体组消息
                _ = Task.Run(async () => {
                    await Task.Delay(1500).ConfigureAwait(false);
                    MediaGroupIDs.Remove(mediaGroupId, out _);

                    await _botClient.EditMessageText(actionMsg, postText, replyMarkup: keyboard).ConfigureAwait(false);
                });
            }
        }

        if (postID > 0)
        {
            //更新附件
            var attachment = _attachmentService.GenerateAttachment(message, postID);
            if (attachment != null)
            {
                await _attachmentService.CreateAttachment(attachment).ConfigureAwait(false);
            }

            //记录媒体组
            await _mediaGroupService.AddPostMediaGroup(message).ConfigureAwait(false);
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
        await Updateable(post).UpdateColumns(static x => new { x.Tags, x.ModifyAt }).ExecuteCommandAsync().ConfigureAwait(false);

        await _botClient.AutoReply($"当前标签: {tagName}", callbackQuery).ConfigureAwait(false);

        bool? hasSpoiler = post.CanSpoiler ? post.HasSpoiler : null;

        var keyboard = post.IsDirectPost ?
            _markupHelperService.DirectPostKeyboard(post.Anonymous, post.Tags, hasSpoiler) :
            _markupHelperService.ReviewKeyboardA(post.Tags, hasSpoiler);
        await _botClient.EditMessageReplyMarkup(callbackQuery.Message!, keyboard).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task SetPostTag(NewPosts post, string payload, CallbackQuery callbackQuery)
    {
        payload = payload.ToLowerInvariant();
        var tag = _tagRepository.GetTagByPayload(payload);
        if (tag != null)
        {
            await SetPostTag(post, tag.Id, callbackQuery).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task RejectPost(NewPosts post, Users dbUser, RejectReasons rejectReason, string? htmlRejectMessage)
    {
        var poster = await _userService.FetchUserByUserID(post.PosterUID).ConfigureAwait(false);

        if (poster == null)
        {
            return;
        }

        if (poster.IsBan)
        {
            await RejectIfBan(post, poster, dbUser, null).ConfigureAwait(false);
            return;
        }
        else
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
            }).ExecuteCommandAsync().ConfigureAwait(false);
        }

        //修改审核群消息
        string reviewMsg = _textHelperService.MakeReviewMessage(poster, dbUser, post.Anonymous, htmlRejectMessage ?? rejectReason.FullText);
        await _botClient.EditMessageText(post.ReviewActionChatID, (int)post.ReviewActionMsgID, reviewMsg, parseMode: ParseMode.Html, disableWebPagePreview: true).ConfigureAwait(false);

        //拒稿频道发布消息
        if (!post.IsMediaGroup)
        {
            if (post.PostType != MessageType.Text)
            {
                var attachment = await _attachmentService.FetchAttachmentByPostId(post.Id).ConfigureAwait(false);

                var inputFile = new InputFileId(attachment.FileID);
                var handler = post.PostType switch {
                    MessageType.Photo => _botClient.SendPhoto(_channelService.RejectChannel.Id, inputFile),
                    MessageType.Audio => _botClient.SendAudio(_channelService.RejectChannel.Id, inputFile),
                    MessageType.Video => _botClient.SendVideo(_channelService.RejectChannel.Id, inputFile),
                    MessageType.Voice => _botClient.SendVoice(_channelService.RejectChannel.Id, inputFile),
                    MessageType.Document => _botClient.SendDocument(_channelService.RejectChannel.Id, inputFile),
                    MessageType.Animation => _botClient.SendAnimation(_channelService.RejectChannel.Id, inputFile),
                    _ => throw new Exception("未知的稿件类型"),
                };

                if (handler != null)
                {
                    await handler.ConfigureAwait(false);
                }
            }
        }
        else
        {
            var attachments = await _attachmentService.FetchAttachmentsByPostId(post.Id).ConfigureAwait(false);
            var group = new IAlbumInputMedia[attachments.Count];
            for (int i = 0; i < attachments.Count; i++)
            {
                var attachmentType = attachments[i].Type;
                if (attachmentType == MessageType.Unknown)
                {
                    attachmentType = post.PostType;
                }
                var inputFile = new InputFileId(attachments[i].FileID);
                group[i] = attachmentType switch {
                    MessageType.Photo => new InputMediaPhoto(inputFile),
                    MessageType.Audio => new InputMediaAudio(inputFile),
                    MessageType.Video => new InputMediaVideo(inputFile),
                    MessageType.Voice => new InputMediaAudio(inputFile),
                    MessageType.Document => new InputMediaDocument(inputFile),
                    _ => throw new Exception("未知的稿件类型"),
                };
            }
            var postMessages = await _botClient.SendMediaGroup(_channelService.RejectChannel, group).ConfigureAwait(false);

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
                }).ExecuteCommandAsync().ConfigureAwait(false);
            }

            //处理媒体组消息
            await _mediaGroupService.AddPostMediaGroup(postMessages).ConfigureAwait(false);
        }

        //通知投稿人
        string posterMsg = _textHelperService.MakeNotification(htmlRejectMessage ?? rejectReason.FullText);
        if (poster.Notification)
        {
            await _botClient.SendMessage(post.OriginChatID, posterMsg, null, (int)post.OriginMsgID, parseMode: ParseMode.Html).ConfigureAwait(false);
        }
        else
        {
            await _botClient.EditMessageText(post.OriginActionChatID, (int)post.OriginActionMsgID, posterMsg, ParseMode.Html, false).ConfigureAwait(false);
        }

        poster.RejectCount++;
        await _userService.UpdateUserPostCount(poster).ConfigureAwait(false);

        if (poster.UserID != dbUser.UserID) //非同一个人才增加审核数量
        {
            dbUser.ReviewCount++;
            await _userService.UpdateUserPostCount(dbUser).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 拒绝封禁用户的投稿
    /// </summary>
    /// <param name="post"></param>
    /// <param name="poster"></param>
    /// <param name="reviewer"></param>
    /// <param name="callbackQuery"></param>
    /// <returns></returns>
    private async Task RejectIfBan(NewPosts post, Users poster, Users reviewer, CallbackQuery? callbackQuery)
    {
        post.RejectReason = "封禁自动拒绝";
        post.CountReject = true;
        post.ReviewerUID = reviewer.UserID;
        post.Status = EPostStatus.Rejected;
        post.ModifyAt = DateTime.Now;
        await Updateable(post).UpdateColumns(static x => new {
            x.RejectReason,
            x.CountReject,
            x.ReviewerUID,
            x.Status,
            x.ModifyAt
        }).ExecuteCommandAsync().ConfigureAwait(false);

        if (callbackQuery != null)
        {
            await _botClient.AutoReply("此用户已被封禁，无法通过审核", callbackQuery).ConfigureAwait(false);

            string reviewMsg = _textHelperService.MakeReviewMessage(poster, reviewer, post.Anonymous, "此用户已被封禁");
            await _botClient.EditMessageText(callbackQuery.Message!, reviewMsg, parseMode: ParseMode.Html, disableWebPagePreview: true).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task AcceptPost(NewPosts post, Users dbUser, bool inPlan, bool second, CallbackQuery callbackQuery)
    {
        var poster = await _userService.FetchUserByUserID(post.PosterUID).ConfigureAwait(false);

        if (poster == null)
        {
            return;
        }

        if (poster.IsBan)
        {
            await RejectIfBan(post, poster, dbUser, callbackQuery).ConfigureAwait(false);
            return;
        }

        ChannelOptions? channel = null;
        if (post.IsFromChannel)
        {
            channel = await _channelOptionService.FetchChannelByChannelId(post.ChannelID).ConfigureAwait(false);
        }
        string postText = _textHelperService.MakePostText(post, poster, channel);

        bool hasSpoiler = post.HasSpoiler;

        Message? publicMsg = null;

        if (!inPlan)
        {
            var acceptChannel = !second ? _channelService.AcceptChannel : _channelService.SecondChannel;

            if (acceptChannel == null)
            {
                _logger.LogError("发布频道为空, 无法发布稿件");
                await _botClient.AutoReply("发布频道为空, 无法发布稿件", callbackQuery, true).ConfigureAwait(false);
                return;
            }

            //发布频道发布消息
            if (!post.IsMediaGroup)
            {
                string? warnText = _tagRepository.GetActivedTagWarnings(post.Tags);
                if (!string.IsNullOrEmpty(warnText))
                {
                    var warnMsg = await _botClient.SendMessage(acceptChannel, warnText, null, null).ConfigureAwait(false);
                    post.WarnTextID = warnMsg.MessageId;
                }

                Message? postMessage = null;
                if (post.PostType == MessageType.Text)
                {
                    postMessage = await _botClient.SendMessageEx(acceptChannel, postText, null, null, ParseMode.Html, !_enableWebPagePreview).ConfigureAwait(false);
                }
                else
                {
                    var attachment = await _attachmentService.FetchAttachmentByPostId(post.Id).ConfigureAwait(false);

                    var inputFile = new InputFileId(attachment.FileID);
                    var handler = post.PostType switch {
                        MessageType.Photo => _botClient.SendPhoto(acceptChannel, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: hasSpoiler),
                        MessageType.Audio => _botClient.SendAudio(acceptChannel, inputFile, caption: postText, parseMode: ParseMode.Html, title: attachment.FileName),
                        MessageType.Video => _botClient.SendVideo(acceptChannel, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: hasSpoiler),
                        MessageType.Voice => _botClient.SendVoice(acceptChannel, inputFile, caption: postText, parseMode: ParseMode.Html),
                        MessageType.Document => _botClient.SendDocument(acceptChannel, inputFile, caption: postText, parseMode: ParseMode.Html),
                        MessageType.Animation => _botClient.SendAnimation(acceptChannel, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: hasSpoiler),
                        _ => null,
                    };

                    if (handler == null)
                    {
                        await _botClient.AutoReply($"不支持的稿件类型: {post.PostType}", callbackQuery).ConfigureAwait(false);
                        return;
                    }

                    postMessage = await handler.ConfigureAwait(false);
                }
                post.PublicMsgID = postMessage?.MessageId ?? -1;
                publicMsg = postMessage;
            }
            else
            {
                var attachments = await _attachmentService.FetchAttachmentsByPostId(post.Id).ConfigureAwait(false);
                var group = new IAlbumInputMedia[attachments.Count];
                for (int i = 0; i < attachments.Count; i++)
                {
                    var attachmentType = attachments[i].Type;
                    if (attachmentType == MessageType.Unknown)
                    {
                        attachmentType = post.PostType;
                    }

                    var inputFile = new InputFileId(attachments[i].FileID);
                    group[i] = attachmentType switch {
                        MessageType.Photo => new InputMediaPhoto(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html, HasSpoiler = hasSpoiler },
                        MessageType.Audio => new InputMediaAudio(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                        MessageType.Video => new InputMediaVideo(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html, HasSpoiler = hasSpoiler },
                        MessageType.Voice => new InputMediaVideo(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                        MessageType.Document => new InputMediaDocument(inputFile) { Caption = i == attachments.Count - 1 ? postText : null, ParseMode = ParseMode.Html },
                        _ => throw new Exception("未知的稿件类型"),
                    };
                }

                string? warnText = _tagRepository.GetActivedTagWarnings(post.Tags);
                if (!string.IsNullOrEmpty(warnText))
                {
                    var warnMsg = await _botClient.SendMessage(acceptChannel, warnText, null, null).ConfigureAwait(false);
                    post.WarnTextID = warnMsg.MessageId;
                }

                var postMessages = await _botClient.SendMediaGroup(acceptChannel, group).ConfigureAwait(false);
                post.PublicMsgID = postMessages.First().MessageId;
                post.PublishMediaGroupID = postMessages.First().MediaGroupId ?? "";
                publicMsg = postMessages.First();

                //记录媒体组消息
                await _mediaGroupService.AddPostMediaGroup(postMessages).ConfigureAwait(false);
            }

            await _botClient.AutoReply("稿件已发布", callbackQuery).ConfigureAwait(false);
            post.Status = !second ? EPostStatus.Accepted : EPostStatus.AcceptedSecond;
        }
        else
        {
            await _botClient.AutoReply("稿件将按设定频率定期发布", callbackQuery).ConfigureAwait(false);
            post.Status = EPostStatus.InPlan;
        }

        post.ReviewerUID = dbUser.UserID;
        post.ModifyAt = DateTime.Now;

        //修改审核群消息
        if (!post.IsDirectPost) // 非直接投稿
        {
            string reviewMsg = _textHelperService.MakeReviewMessage(poster, dbUser, post.Anonymous, second, publicMsg);
            await _botClient.EditMessageText(callbackQuery.Message!, reviewMsg, parseMode: ParseMode.Html, disableWebPagePreview: true).ConfigureAwait(false);
        }
        else // 直接投稿, 在审核群留档
        {
            string reviewMsg = _textHelperService.MakeReviewMessage(poster, post.Anonymous, second, publicMsg);
            var msg = await _botClient.SendMessage(_channelService.ReviewGroup.Id, reviewMsg, null, null, parseMode: ParseMode.Html, disableWebPagePreview: !_enableWebPagePreview).ConfigureAwait(false);
            post.ReviewMsgID = msg.MessageId;
        }

        await Updateable(post).UpdateColumns(static x => new {
            x.ReviewMsgID,
            x.PublicMsgID,
            x.PublishMediaGroupID,
            x.ReviewerUID,
            x.WarnTextID,
            x.Status,
            x.ModifyAt
        }).ExecuteCommandAsync().ConfigureAwait(false);

        //通知投稿人
        string posterMsg = _textHelperService.MakeNotification(post.IsDirectPost, inPlan, publicMsg);
        if (poster.Notification && poster.UserID != dbUser.UserID)//启用通知并且审核与投稿不是同一个人
        {
            //单独发送通知消息
            await _botClient.SendMessage(post.OriginChatID, posterMsg, null, (int)post.OriginMsgID, parseMode: ParseMode.Html, disableWebPagePreview: true).ConfigureAwait(false);
        }
        else
        {
            //静默模式, 不单独发送通知消息
            await _botClient.EditMessageText(post.OriginChatID, (int)post.OriginActionMsgID, posterMsg, ParseMode.Html, disableWebPagePreview: true).ConfigureAwait(false);
        }

        //增加通过数量
        poster.AcceptCount++;
        poster.ModifyAt = DateTime.Now;
        await _userService.UpdateUserPostCount(poster).ConfigureAwait(false);

        if (!post.IsDirectPost) //增加审核数量
        {
            if (poster.UserID != dbUser.UserID)
            {
                dbUser.ReviewCount++;
                await _userService.UpdateUserPostCount(dbUser).ConfigureAwait(false);
            }
        }
        else
        {
            poster.PostCount++;
            await _userService.UpdateUserPostCount(poster).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> PublicInPlanPost(NewPosts post)
    {
        var poster = await _userService.FetchUserByUserID(post.PosterUID).ConfigureAwait(false);

        if (poster == null)
        {
            return false;
        }

        if (post.IsDirectPost)
        {
            poster.PostCount++;
        }

        ChannelOptions? channel = null;
        if (post.IsFromChannel)
        {
            channel = await _channelOptionService.FetchChannelByChannelId(post.ChannelID).ConfigureAwait(false);
        }
        string postText = _textHelperService.MakePostText(post, poster, channel);
        bool hasSpoiler = post.HasSpoiler;

        try
        {
            //发布频道发布消息
            if (!post.IsMediaGroup)
            {
                string? warnText = _tagRepository.GetActivedTagWarnings(post.Tags);
                if (!string.IsNullOrEmpty(warnText))
                {
                    var warnMsg = await _botClient.SendMessage(_channelService.AcceptChannel.Id, warnText).ConfigureAwait(false);
                    post.WarnTextID = warnMsg.MessageId;
                }

                Message? postMessage = null;
                if (post.PostType == MessageType.Text)
                {
                    postMessage = await _botClient.SendMessage(_channelService.AcceptChannel.Id, postText, null, null, parseMode: ParseMode.Html, disableWebPagePreview: true).ConfigureAwait(false);
                }
                else
                {
                    var attachment = await _attachmentService.FetchAttachmentByPostId(post.Id).ConfigureAwait(false);

                    var inputFile = new InputFileId(attachment.FileID);
                    var handler = post.PostType switch {
                        MessageType.Photo => _botClient.SendPhoto(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: hasSpoiler),
                        MessageType.Audio => _botClient.SendAudio(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html, title: attachment.FileName),
                        MessageType.Video => _botClient.SendVideo(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: hasSpoiler),
                        MessageType.Voice => _botClient.SendVoice(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html),
                        MessageType.Document => _botClient.SendDocument(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html),
                        MessageType.Animation => _botClient.SendAnimation(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: hasSpoiler),
                        _ => null,
                    };

                    if (handler == null)
                    {
                        _logger.LogError("不支持的稿件类型: {postType}", post.PostType);
                        return false;
                    }

                    postMessage = await handler.ConfigureAwait(false);
                }
                post.PublicMsgID = postMessage?.MessageId ?? -1;
            }
            else
            {
                var attachments = await _attachmentService.FetchAttachmentsByPostId(post.Id).ConfigureAwait(false);
                var group = new IAlbumInputMedia[attachments.Count];
                for (int i = 0; i < attachments.Count; i++)
                {
                    var attachmentType = attachments[i].Type;
                    if (attachmentType == MessageType.Unknown)
                    {
                        attachmentType = post.PostType;
                    }

                    var inputFile = new InputFileId(attachments[i].FileID);
                    group[i] = attachmentType switch {
                        MessageType.Photo => new InputMediaPhoto(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html, HasSpoiler = hasSpoiler },
                        MessageType.Audio => new InputMediaAudio(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                        MessageType.Video => new InputMediaVideo(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html, HasSpoiler = hasSpoiler },
                        MessageType.Voice => new InputMediaVideo(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                        MessageType.Document => new InputMediaDocument(inputFile) { Caption = i == attachments.Count - 1 ? postText : null, ParseMode = ParseMode.Html },
                        _ => throw new Exception("未知的稿件类型"),
                    };
                }

                string? warnText = _tagRepository.GetActivedTagWarnings(post.Tags);
                if (!string.IsNullOrEmpty(warnText))
                {
                    var warnMsg = await _botClient.SendMessage(_channelService.AcceptChannel, warnText).ConfigureAwait(false);
                    post.WarnTextID = warnMsg.MessageId;
                }

                var postMessages = await _botClient.SendMediaGroup(_channelService.AcceptChannel, group).ConfigureAwait(false);
                post.PublicMsgID = postMessages.First().MessageId;
                post.PublishMediaGroupID = postMessages.First().MediaGroupId ?? "";

                //记录媒体组消息
                await _mediaGroupService.AddPostMediaGroup(postMessages).ConfigureAwait(false);
            }
        }
        finally
        {
            post.Status = EPostStatus.Accepted;
            post.ModifyAt = DateTime.Now;

            await Updateable(post).UpdateColumns(static x => new {
                x.PublicMsgID,
                x.PublishMediaGroupID,
                x.Status,
                x.ModifyAt
            }).ExecuteCommandAsync().ConfigureAwait(false);
        }
        return true;
    }

    /// <inheritdoc/>
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
            ).ConfigureAwait(false);
        }
        else
        {
            post = await Queryable().FirstAsync(x => x.OriginMediaGroupID == msgGroupId || x.ReviewMediaGroupID == msgGroupId).ConfigureAwait(false);
        }

        return post;
    }

    /// <inheritdoc/>
    public async Task<NewPosts?> FetchPostFromCallbackQuery(CallbackQuery query)
    {
        if (query.Message == null)
        {
            return null;
        }
        var post = await FetchPostFromReplyToMessage(query.Message).ConfigureAwait(false);
        return post;
    }

    /// <inheritdoc/>
    public async Task<NewPosts?> GetLatestReviewingPostLink()
    {
        var now = DateTime.Now;
        var today = now.AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);

        var post = await Queryable().Where(x => x.CreateAt >= today && x.Status == EPostStatus.Reviewing).FirstAsync().ConfigureAwait(false);
        return post;
    }

    /// <inheritdoc/>
    public async Task<NewPosts?> GetPostByPostId(int postId)
    {
        return await Queryable().FirstAsync(x => x.Id == postId).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task<int> CountAllPosts()
    {
        return Queryable().Where(x => x.Status > EPostStatus.Cancel).CountAsync();
    }

    /// <inheritdoc/>
    public Task<int> CountAllPosts(DateTime afterTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.Status > EPostStatus.Cancel).CountAsync();
    }

    /// <inheritdoc/>
    public Task<int> CountAllPosts(DateTime afterTime, DateTime beforeTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.CreateAt < beforeTime && x.Status > EPostStatus.Cancel).CountAsync();
    }

    /// <inheritdoc/>
    public Task<int> CountAcceptedPosts()
    {
        return Queryable().Where(x => x.Status == EPostStatus.Accepted).CountAsync();
    }

    /// <inheritdoc/>
    public Task<int> CountAcceptedPosts(DateTime afterTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.Status == EPostStatus.Accepted).CountAsync();
    }

    /// <inheritdoc/>
    public Task<int> CountAcceptedPosts(DateTime afterTime, DateTime beforeTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.CreateAt < beforeTime && x.Status == EPostStatus.Accepted).CountAsync();
    }

    /// <inheritdoc/>
    public Task<int> CountAcceptedSecondPosts()
    {
        return Queryable().Where(x => x.Status == EPostStatus.AcceptedSecond).CountAsync();
    }

    /// <inheritdoc/>
    public Task<int> CountAcceptedSecondPosts(DateTime afterTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.Status == EPostStatus.AcceptedSecond).CountAsync();
    }

    /// <inheritdoc/>
    public Task<int> CountAcceptedSecondPosts(DateTime afterTime, DateTime beforeTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.CreateAt < beforeTime && x.Status == EPostStatus.AcceptedSecond).CountAsync();
    }

    /// <inheritdoc/>
    public Task<int> CountRejectedPosts()
    {
        return Queryable().Where(x => x.Status == EPostStatus.Rejected).CountAsync();
    }

    /// <inheritdoc/>
    public Task<int> CountRejectedPosts(DateTime afterTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.Status == EPostStatus.Rejected).CountAsync();
    }

    /// <inheritdoc/>
    public Task<int> CountRejectedPosts(DateTime afterTime, DateTime beforeTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.CreateAt < beforeTime && x.Status == EPostStatus.Rejected).CountAsync();
    }

    /// <inheritdoc/>
    public Task<int> CountExpiredPosts()
    {
        return Queryable().Where(x => x.Status < 0).CountAsync();
    }

    /// <inheritdoc/>
    public Task<int> CountExpiredPosts(DateTime afterTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.Status < 0).CountAsync();
    }

    /// <inheritdoc/>
    public Task<int> CountReviewingPosts(DateTime afterTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.Status == EPostStatus.Reviewing).CountAsync();
    }

    /// <inheritdoc/>
    public Task<int> CountReviewingPosts(DateTime afterTime, DateTime beforeTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.CreateAt < beforeTime && x.Status == EPostStatus.Reviewing).CountAsync();
    }

    /// <inheritdoc/>
    public Task RevocationPost(NewPosts post)
    {
        post.Status = EPostStatus.Revocation;
        post.ModifyAt = DateTime.Now;
        return Updateable(post).UpdateColumns(static x => new { x.Status, x.ModifyAt }).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public Task CancelPost(NewPosts post)
    {
        post.Status = EPostStatus.Cancel;
        post.ModifyAt = DateTime.Now;
        return Updateable(post).UpdateColumns(static x => new { x.Status, x.ModifyAt }).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public Task EditPostText(NewPosts post, string text)
    {
        post.Text = text;
        post.ModifyAt = DateTime.Now;
        return Updateable(post).UpdateColumns(static x => new { x.Text }).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public Task SetPostAnonymous(NewPosts post, bool anonymous)
    {
        post.Anonymous = anonymous;
        post.ModifyAt = DateTime.Now;
        return Updateable(post).UpdateColumns(static x => new { x.Anonymous, x.ModifyAt }).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public Task SetPostForceAnonymous(NewPosts post, bool anonymous)
    {
        post.ForceAnonymous = anonymous;
        post.ModifyAt = DateTime.Now;
        return Updateable(post).UpdateColumns(static x => new { x.ForceAnonymous, x.ModifyAt }).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public Task SetPostSpoiler(NewPosts post, bool spoiler)
    {
        post.HasSpoiler = spoiler;
        post.ModifyAt = DateTime.Now;
        return Updateable(post).UpdateColumns(static x => new { x.HasSpoiler, x.ModifyAt }).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public Task<bool> IfExistsMediaGroupId(string mediaGroupId)
    {
        return Queryable().AnyAsync(x => x.OriginMediaGroupID == mediaGroupId);
    }

    /// <inheritdoc/>
    public async Task<NewPosts?> GetRandomPost()
    {
        return await Queryable()
                    .Where(static x => x.Status == EPostStatus.Accepted && x.PostType == MessageType.Photo)
                    .OrderBy(static x => SqlFunc.GetRandom()).FirstAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task<NewPosts> GetInPlanPost()
    {
        return Queryable().Where(static x => x.Status == EPostStatus.InPlan).FirstAsync();
    }

    /// <inheritdoc/>
    public Task UpdatePostStatus(NewPosts post, EPostStatus status)
    {
        post.Status = status;
        post.ModifyAt = DateTime.Now;
        return Updateable(post).UpdateColumns(static x => new { x.Status, x.ModifyAt }).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public Task<int> CreateNewPosts(NewPosts post)
    {
        return Insertable(post).ExecuteReturnIdentityAsync();
    }

    /// <inheritdoc/>
    public Task<List<NewPosts>> GetExpiredPosts(DateTime beforeTime)
    {
        return Queryable()
            .Where(x => (x.Status == EPostStatus.Padding || x.Status == EPostStatus.Reviewing) && x.ModifyAt < beforeTime)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public Task<List<NewPosts>> GetExpiredPosts(long userID, DateTime beforeTime)
    {
        return Queryable()
            .Where(x => x.PosterUID == userID && (x.Status == EPostStatus.Padding || x.Status == EPostStatus.Reviewing) && x.ModifyAt < beforeTime)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public void Dispose() => MediaGroupTtlTimer?.Dispose();
}

