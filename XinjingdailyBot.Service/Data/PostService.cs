using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
internal sealed class PostService(
    ILogger<PostService> _logger,
    IAttachmentService _attachmentService,
    IChannelService _channelService,
    IChannelOptionService _channelOptionService,
    ITextHelperService _textHelperService,
    IMarkupHelperService _markupHelperService,
    ITelegramBotClient _botClient,
    IUserService _userService,
    IOptions<OptionsSetting> options,
    TagRepository _tagRepository,
    IMediaGroupService _mediaGroupService,
    ISqlSugarClient _context) : BaseService<NewPosts>(_context), IPostService
{
    private readonly OptionsSetting.PostOption _postOption = options.Value.Post;
    private readonly bool _enableWebPagePreview = options.Value.Bot.EnableWebPagePreview;

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
                return false;
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

        if (!await ProcessMessage(message))
        {
            return;
        }

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

        var actionMsg = await _botClient.SendTextMessageAsync(message.Chat, postText, replyToMessageId: message.MessageId, replyMarkup: keyboard, allowSendingWithoutReply: true);

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

        if (!await ProcessMessage(message))
        {
            return;
        }

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

        var actionMsg = await _botClient.SendTextMessageAsync(message.Chat, postText, replyToMessageId: message.MessageId, replyMarkup: keyboard, allowSendingWithoutReply: true);

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
            await _attachmentService.CreateAttachment(attachment);
        }
    }

    class MediaGroupData
    {
        public int id = -1;
        public DateTime lastActive = DateTime.Now;
        public bool cancelled = false;
    }

    /// <summary>
    /// mediaGroupID字典
    /// </summary>
    private ConcurrentDictionary<string, MediaGroupData> MediaGroupIDs { get; } = new();

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
        if (!MediaGroupIDs.TryGetValue(mediaGroupId, out var post)) //如果mediaGroupId不存在则创建新Post
        {
            MediaGroupIDs.TryAdd(mediaGroupId, new MediaGroupData());
            post = MediaGroupIDs[mediaGroupId];

            bool exists = await Queryable().AnyAsync(x => x.OriginMediaGroupID == mediaGroupId);
            if (!exists)
            {
                await _botClient.SendChatActionAsync(message, ChatAction.Typing);

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

                var actionMsg = await _botClient.SendTextMessageAsync(message.Chat, "处理中, 请稍后", replyToMessageId: message.MessageId, allowSendingWithoutReply: true);


                if (!await ProcessMessage(message))
                {
                    await _botClient.DeleteMessageAsync(actionMsg.Chat, actionMsg.MessageId);
                    return;
                }

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
                    newPost.ReviewMediaGroupID = mediaGroupId;
                }

                post.id = await Insertable(newPost).ExecuteReturnIdentityAsync();
                post.lastActive = DateTime.Now;
                MediaGroupIDs[mediaGroupId] = post;

                // 0.3 秒无新消息则停止接收媒体组消息
                _ = Task.Run(async () => {
                    while (!MediaGroupIDs[mediaGroupId].cancelled && DateTime.Now - MediaGroupIDs[mediaGroupId].lastActive < TimeSpan.FromSeconds(.3))
                    {
                        await Task.Delay(60);
                    }

                    MediaGroupIDs.Remove(mediaGroupId, out var group);
                    if (group.cancelled)
                    {
                        await _botClient.DeleteMessageAsync(actionMsg.Chat, actionMsg.MessageId);
                    } else
                    {
                        MediaGroupIDs.Remove(mediaGroupId, out _);
                        await _botClient.EditMessageTextAsync(actionMsg, postText, replyMarkup: keyboard);
                    }
                });
            }
        }

        if (post != null)
        {
            post.lastActive = DateTime.Now;

            if(await ProcessMessage(message))
            {
                //更新附件
                var attachment = _attachmentService.GenerateAttachment(message, post.id);
                if (attachment != null)
                {
                    await _attachmentService.CreateAttachment(attachment);
                }

                //记录媒体组
                await _mediaGroupService.AddPostMediaGroup(message);
            } else
            {
                post.cancelled = true;
            }
        }
    }

    private async Task<Boolean> ProcessMessage(Message msg)
    {
        if (msg.Photo != null)
        {
            var size = msg.Photo.Last();
            double ratio = ((double)size.Width) / size.Height;
            if (ratio < 0.3)
            {
                await _botClient.SendTextMessageAsync(msg.Chat, "长图清晰度过低，请将其以文件模式发送，以切分此图片。\n\n在 PC 客户端上，拖入图片后取消 “压缩图片” 或 “图片格式” 选项即可以文件格式发送\n在 安卓 客户端上，长按发送按钮，点击文件图标即可以文件格式发送。", replyToMessageId: msg.MessageId);
                return false;
            }

            if(ratio > 4.5)
            {
                await _botClient.SendTextMessageAsync(msg.Chat, "图片过宽，建议将其以文件模式发送，以自动调整宽高比。", replyToMessageId: msg.MessageId);
            }
        }

        if(msg.Document != null)
        {
            if(msg.Document.MimeType.StartsWith("image/"))
            {
                var tipsMsg = await _botClient.SendTextMessageAsync(msg.Chat, "正在处理，请稍候……", replyToMessageId: msg.MessageId);
                // 切分图像
                Stream fileStream = new MemoryStream();
                await _botClient.GetInfoAndDownloadFileAsync(msg.Document.FileId, fileStream);
                var originImg = new Bitmap(Image.FromStream(fileStream));
                var originRatio = originImg.Width / originImg.Height;
                if (originRatio < 0.4)
                {
                    // split image
                    var imgs = new List<IAlbumInputMedia>();
                    const double splitTargetRatio = 9.0 / 12.0; // 目标宽高比
                    int splitMidHeight = (int)Math.Round(originImg.Width / splitTargetRatio); // 每张高度（实际高度 midHeight + scanHeight * k, k∈[-1, 1]）
                    int splitPadding = (int)(0.05 * splitMidHeight); // 每张上下重复高度
                    int splitScanHeight = (int)(0.3 * splitMidHeight); // 上下扫描切分点高度
                    int splicScanHorizontal = (int)(0.01 * originImg.Width); // 横向扫描距离

                    int currentY = 0;
                    while (currentY < originImg.Height)
                    {
                        int scanStartY = Math.Max(1, currentY + splitMidHeight - splitScanHeight);
                        int scanEndY = Math.Min((int)originImg.Height, (currentY + splitMidHeight + splitScanHeight));

                        int maxDiffY = 0;
                        double maxDiff = -100;

                        if (originImg.Height - currentY - splitPadding - splitScanHeight - splitMidHeight > 0)
                            for (int y = scanStartY; y < scanEndY; y++)
                            {
                                double diff = 0;

                                for (int x = 0; x < originImg.Width; x++)
                                {
                                    var p1 = originImg.GetPixel(x, y);

                                    double minDiffPixel = 99999;
                                    for (int qx = Math.Max(0, x - splicScanHorizontal); qx < Math.Min(x + splicScanHorizontal, originImg.Width - 1); qx++)
                                    {
                                        var p2 = originImg.GetPixel(qx, y - 1);
                                        double diffPixel = Math.Sqrt(
                                            Math.Pow(p1.R - p2.R, 2) +
                                            Math.Pow(p1.G - p2.G, 2) +
                                            Math.Pow(p1.B - p2.B, 2)
                                            );

                                        minDiffPixel = Math.Min(minDiffPixel, diffPixel);
                                    }

                                    diff += minDiffPixel;
                                }

                                if (diff > maxDiff)
                                {
                                    maxDiff = diff;
                                    maxDiffY = y;
                                }
                            }
                        else maxDiffY = originImg.Height;
                        var height = Math.Min(maxDiffY - currentY + splitPadding * (currentY == 0 ? 1 : 2), originImg.Height - currentY + splitPadding);


                        var img = new Bitmap(originImg.Width, height);
                        Graphics g = Graphics.FromImage(img);
                        g.Clear(System.Drawing.Color.White);
                        g.DrawImage(originImg, new Point(0, (currentY == 0 ? 0 : -currentY + splitPadding)));
                        g.Dispose();

                        var memoryStream = new MemoryStream();
                        img.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                        img.Dispose();
                        memoryStream.Position = 0;
                        imgs.Add(new InputMediaPhoto(new InputFileStream(memoryStream, $"image{imgs.Count}.png")));

                        currentY = maxDiffY;
                    }

                    for (int i = 0; i < Math.Ceiling((double)imgs.Count / 9); i++)
                    {
                        await _botClient.SendMediaGroupAsync(msg.Chat, imgs.Slice(i * 9, Math.Min(9, imgs.Count - i * 9)), replyToMessageId: msg.MessageId);
                    }

                    fileStream.Close();
                    originImg.Dispose();

                    await _botClient.DeleteMessageAsync(tipsMsg.Chat, tipsMsg.MessageId);
                    await _botClient.SendTextMessageAsync(msg.Chat, "图片切分处理完成，请选择要投稿的图片并转发给机器人。", replyToMessageId: msg.MessageId);
                }
                else if (originRatio > 2)
                {
                    const double splitTargetRatio = 2; // 目标宽高比
                    int targetHeight = (int)(originImg.Width / splitTargetRatio);
                    int paintY = (int)(targetHeight / 2 - originImg.Height / 2);

                    var img = new Bitmap(originImg.Width, targetHeight);
                    Graphics g = Graphics.FromImage(img);
                    g.Clear(System.Drawing.Color.White);
                    var imgBlurred = ConvolutionFilter(originImg, new double[,]
                { {  2, 04, 05, 04, 2 },
                  {  4, 09, 12, 09, 4 },
                  {  5, 12, 15, 12, 5 },
                  {  4, 09, 12, 09, 4 },
                  {  2, 04, 05, 04, 2 }, }, 1.0 / 159.0);
                    var imgBlurredBlurred = ConvolutionFilter(imgBlurred, new double[,]
                { {  2, 04, 05, 04, 2 },
                  {  4, 09, 12, 09, 4 },
                  {  5, 12, 15, 12, 5 },
                  {  4, 09, 12, 09, 4 },
                  {  2, 04, 05, 04, 2 }, }, 1.0 / 159.0);
                    var scale = targetHeight / originImg.Height * 1.6;
                    g.DrawImage(imgBlurredBlurred, (int)((originImg.Width * scale - originImg.Width) * -0.5),(int)(-0.15 * targetHeight), (int)(originImg.Width * scale), (int)(originImg.Height * scale));
                    g.DrawImage(originImg, new Point(0, paintY));
                    g.Dispose();
                    var memoryStream = new MemoryStream();
                    img.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    img.Dispose();
                    imgBlurred.Dispose();
                    imgBlurredBlurred.Dispose();
                    memoryStream.Position = 0;
                    await _botClient.SendPhotoAsync(msg.Chat, new InputFileStream(memoryStream), replyToMessageId: msg.MessageId);
                    await _botClient.DeleteMessageAsync(tipsMsg.Chat, tipsMsg.MessageId);
                    await _botClient.SendTextMessageAsync(msg.Chat, "图片处理完成，请选择要投稿的图片并转发给机器人。", replyToMessageId: msg.MessageId);
                }
                return false;
            }
        }

        if(msg.Text != null) {
            if(msg.Document == null && msg.Photo == null && msg.Audio == null && msg.Video == null)
            {
                // 纯链接检测
                var linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                foreach (Match m in linkParser.Matches(msg.Text))
                {
                    var uri = new Uri(m.Value);
                    var dict = new Dictionary<string, string> {
                        {"b23.tv", "@bilifeedbot"},
                        {"bilibili.com", "@bilifeedbot"},
                        {"twitter.com", "@TwPicBot"},
                        {"x.com", "@TwPicBot" },
                        {"fxtwitter.com", "@TwPicBot" },
                        {"fixupx.com", "@TwPicBot" },
                        {"fixvx.com", "@TwPicBot" },
                        {"twittpr.com","@TwPicBot" },
                        {"weibo.com", "@web2album_bot" },
                        {"xiaohongshu.com", "@web2album_bot" },
                        {"douyin.com", "@icbcbot" },
                        {"youtube.com", "@icbcbot" },
                        {"youtu.be", "@icbcbot" },
                        {"pixiv.net", "@Pixiv_bot" },
                        {"pximg.net", "@Pixiv_bot" }
                    };

                    foreach(var host in dict)
                    {
                        if(uri.Host.EndsWith(host.Key))
                        {
                            await _botClient.SendTextMessageAsync(msg.Chat, $"检测到来自 {host.Key} 的纯链接投稿，请先将链接发送至 {host.Value} 进行处理后再投稿。", replyToMessageId: msg.MessageId);
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }

    // Taken from https://softwarebydefault.com/2013/06/09/image-blur-filters/
    public static Bitmap ConvolutionFilter(Bitmap sourceBitmap, double[,] filterMatrix, double factor = 1, int bias = 0)
    {
        BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
                                 sourceBitmap.Width, sourceBitmap.Height),
                                                   ImageLockMode.ReadOnly,
                                             System.Drawing.Imaging.PixelFormat.Format32bppArgb);


        byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
        byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];


        Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
        sourceBitmap.UnlockBits(sourceData);

        double blue = 0.0;
        double green = 0.0;
        double red = 0.0;

        int filterWidth = filterMatrix.GetLength(1);
        int filterHeight = filterMatrix.GetLength(0);

        int filterOffset = (filterWidth - 1) / 2;
        int calcOffset = 0;

        int byteOffset = 0;

        for (int offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++)
        {
            for (int offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++)
            {
                blue = 0;
                green = 0;
                red = 0;

                byteOffset = offsetY * sourceData.Stride + offsetX * 4;

                for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                {
                    for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                    {
                        calcOffset = byteOffset +
                                     (filterX * 4) +
                                     (filterY * sourceData.Stride);

                        blue += (double)(pixelBuffer[calcOffset]) *
                                filterMatrix[filterY + filterOffset,
                                                    filterX + filterOffset];

                        green += (double)(pixelBuffer[calcOffset + 1]) *
                                 filterMatrix[filterY + filterOffset,
                                                    filterX + filterOffset];

                        red += (double)(pixelBuffer[calcOffset + 2]) *
                               filterMatrix[filterY + filterOffset,
                                                  filterX + filterOffset];
                    }
                }

                blue = factor * blue + bias;
                green = factor * green + bias;
                red = factor * red + bias;

                blue = (blue > 255 ? 255 : (blue < 0 ? 0 : blue));

                green = (green > 255 ? 255 : (green < 0 ? 0 : green));

                red = (red > 255 ? 255 : (red < 0 ? 0 : red));

                const double brightness = 1 / 1.414;

                resultBuffer[byteOffset] = (byte)(blue * brightness);
                resultBuffer[byteOffset + 1] = (byte)(green * brightness);
                resultBuffer[byteOffset + 2] = (byte)(red * brightness);

                resultBuffer[byteOffset + 3] = 255;
            }
        }

        Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

        BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height), ImageLockMode.WriteOnly,
                                             System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
        resultBitmap.UnlockBits(resultData);

        return resultBitmap;
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

    public async Task RejectPost(NewPosts post, Users dbUser, RejectReasons rejectReason, string? htmlRejectMessage)
    {
        var poster = await _userService.FetchUserByUserID(post.PosterUID);

        if (poster == null)
        {
            return;
        }

        if (poster.IsBan)
        {
            await RejectIfBan(post, poster, dbUser, null);
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
            }).ExecuteCommandAsync();
        }

        //修改审核群消息
        string reviewMsg = _textHelperService.MakeReviewMessage(poster, dbUser, post.Anonymous, htmlRejectMessage ?? rejectReason.FullText);
        await _botClient.EditMessageTextAsync(post.ReviewActionChatID, (int)post.ReviewActionMsgID, reviewMsg, parseMode: ParseMode.Html, disableWebPagePreview: true);

        //拒稿频道发布消息
        if (!post.IsMediaGroup)
        {
            if (post.PostType != MessageType.Text)
            {
                var attachment = await _attachmentService.FetchAttachmentByPostId(post.Id);

                var inputFile = new InputFileId(attachment.FileID);
                var handler = post.PostType switch {
                    MessageType.Photo => _botClient.SendPhotoAsync(_channelService.RejectChannel.Id, inputFile),
                    MessageType.Audio => _botClient.SendAudioAsync(_channelService.RejectChannel.Id, inputFile),
                    MessageType.Video => _botClient.SendVideoAsync(_channelService.RejectChannel.Id, inputFile),
                    MessageType.Voice => _botClient.SendVoiceAsync(_channelService.RejectChannel.Id, inputFile),
                    MessageType.Document => _botClient.SendDocumentAsync(_channelService.RejectChannel.Id, inputFile),
                    MessageType.Animation => _botClient.SendAnimationAsync(_channelService.RejectChannel.Id, inputFile),
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
            var attachments = await _attachmentService.FetchAttachmentsByPostId(post.Id);
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
            var postMessages = await _botClient.SendMediaGroupAsync(_channelService.RejectChannel, group);

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
        string posterMsg = _textHelperService.MakeNotification(htmlRejectMessage ?? rejectReason.FullText);
        if (poster.Notification)
        {
            await _botClient.SendTextMessageAsync(post.OriginChatID, posterMsg, parseMode: ParseMode.Html, replyToMessageId: (int)post.OriginMsgID, allowSendingWithoutReply: true);
        }
        else
        {
            await _botClient.EditMessageTextAsync(post.OriginActionChatID, (int)post.OriginActionMsgID, posterMsg, parseMode: ParseMode.Html);
        }

        poster.RejectCount++;
        await _userService.UpdateUserPostCount(poster);

        if (poster.UserID != dbUser.UserID) //非同一个人才增加审核数量
        {
            dbUser.ReviewCount++;
            await _userService.UpdateUserPostCount(dbUser);
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
        }).ExecuteCommandAsync();

        if (callbackQuery != null)
        {
            await _botClient.AutoReplyAsync("此用户已被封禁，无法通过审核", callbackQuery);

            string reviewMsg = _textHelperService.MakeReviewMessage(poster, reviewer, post.Anonymous, "此用户已被封禁");
            await _botClient.EditMessageTextAsync(callbackQuery.Message!, reviewMsg, parseMode: ParseMode.Html, disableWebPagePreview: true);
        }
    }

    public async Task AcceptPost(NewPosts post, Users dbUser, bool inPlan, bool second, CallbackQuery callbackQuery)
    {
        var poster = await _userService.FetchUserByUserID(post.PosterUID);

        if (poster == null)
        {
            return;
        }

        if (poster.IsBan)
        {
            await RejectIfBan(post, poster, dbUser, callbackQuery);
            return;
        }

        ChannelOptions? channel = null;
        if (post.IsFromChannel)
        {
            channel = await _channelOptionService.FetchChannelByChannelId(post.ChannelID);
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
                await _botClient.AutoReplyAsync("发布频道为空, 无法发布稿件", callbackQuery, true);
                return;
            }

            //发布频道发布消息
            if (!post.IsMediaGroup)
            {
                string? warnText = _tagRepository.GetActivedTagWarnings(post.Tags);
                if (!string.IsNullOrEmpty(warnText))
                {
                    var warnMsg = await _botClient.SendTextMessageAsync(acceptChannel, warnText, allowSendingWithoutReply: true);
                    post.WarnTextID = warnMsg.MessageId;
                }

                Message? postMessage = null;
                if (post.PostType == MessageType.Text)
                {
                    postMessage = await _botClient.SendTextMessageAsync(acceptChannel, postText, parseMode: ParseMode.Html, disableWebPagePreview: !_enableWebPagePreview);
                }
                else
                {
                    var attachment = await _attachmentService.FetchAttachmentByPostId(post.Id);

                    var inputFile = new InputFileId(attachment.FileID);
                    var handler = post.PostType switch {
                        MessageType.Photo => _botClient.SendPhotoAsync(acceptChannel, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: hasSpoiler),
                        MessageType.Audio => _botClient.SendAudioAsync(acceptChannel, inputFile, caption: postText, parseMode: ParseMode.Html, title: attachment.FileName),
                        MessageType.Video => _botClient.SendVideoAsync(acceptChannel, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: hasSpoiler),
                        MessageType.Voice => _botClient.SendVoiceAsync(acceptChannel, inputFile, caption: postText, parseMode: ParseMode.Html),
                        MessageType.Document => _botClient.SendDocumentAsync(acceptChannel, inputFile, caption: postText, parseMode: ParseMode.Html),
                        MessageType.Animation => _botClient.SendAnimationAsync(acceptChannel, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: hasSpoiler),
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
                publicMsg = postMessage;
            }
            else
            {
                var attachments = await _attachmentService.FetchAttachmentsByPostId(post.Id);
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
                    var warnMsg = await _botClient.SendTextMessageAsync(acceptChannel, warnText, allowSendingWithoutReply: true);
                    post.WarnTextID = warnMsg.MessageId;
                }

                var postMessages = await _botClient.SendMediaGroupAsync(acceptChannel, group);
                post.PublicMsgID = postMessages.First().MessageId;
                post.PublishMediaGroupID = postMessages.First().MediaGroupId ?? "";
                publicMsg = postMessages.First();

                //记录媒体组消息
                await _mediaGroupService.AddPostMediaGroup(postMessages);
            }

            await _botClient.AutoReplyAsync("稿件已发布", callbackQuery);
            post.Status = !second ? EPostStatus.Accepted : EPostStatus.AcceptedSecond;
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
            string reviewMsg = _textHelperService.MakeReviewMessage(poster, dbUser, post.Anonymous, second, publicMsg);
            await _botClient.EditMessageTextAsync(callbackQuery.Message!, reviewMsg, parseMode: ParseMode.Html, disableWebPagePreview: true);
        }
        else // 直接投稿, 在审核群留档
        {
            string reviewMsg = _textHelperService.MakeReviewMessage(poster, post.Anonymous, second, publicMsg);
            var msg = await _botClient.SendTextMessageAsync(_channelService.ReviewGroup.Id, reviewMsg, parseMode: ParseMode.Html, disableWebPagePreview: !_enableWebPagePreview);
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
        }).ExecuteCommandAsync();

        //通知投稿人
        string posterMsg = _textHelperService.MakeNotification(post.IsDirectPost, inPlan, publicMsg);
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
        await _userService.UpdateUserPostCount(poster);

        if (!post.IsDirectPost) //增加审核数量
        {
            if (poster.UserID != dbUser.UserID)
            {
                dbUser.ReviewCount++;
                await _userService.UpdateUserPostCount(dbUser);
            }
        }
        else
        {
            poster.PostCount++;
            await _userService.UpdateUserPostCount(poster);
        }
    }

    public async Task<bool> PublicInPlanPost(NewPosts post)
    {
        var poster = await _userService.FetchUserByUserID(post.PosterUID);

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
            channel = await _channelOptionService.FetchChannelByChannelId(post.ChannelID);
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
                    var warnMsg = await _botClient.SendTextMessageAsync(_channelService.AcceptChannel.Id, warnText, allowSendingWithoutReply: true);
                    post.WarnTextID = warnMsg.MessageId;
                }

                Message? postMessage = null;
                if (post.PostType == MessageType.Text)
                {
                    postMessage = await _botClient.SendTextMessageAsync(_channelService.AcceptChannel.Id, postText, parseMode: ParseMode.Html, disableWebPagePreview: true);
                }
                else
                {
                    var attachment = await _attachmentService.FetchAttachmentByPostId(post.Id);

                    var inputFile = new InputFileId(attachment.FileID);
                    var handler = post.PostType switch {
                        MessageType.Photo => _botClient.SendPhotoAsync(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: hasSpoiler),
                        MessageType.Audio => _botClient.SendAudioAsync(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html, title: attachment.FileName),
                        MessageType.Video => _botClient.SendVideoAsync(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: hasSpoiler),
                        MessageType.Voice => _botClient.SendVoiceAsync(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html),
                        MessageType.Document => _botClient.SendDocumentAsync(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html),
                        MessageType.Animation => _botClient.SendAnimationAsync(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: hasSpoiler),
                        _ => null,
                    };

                    if (handler == null)
                    {
                        _logger.LogError("不支持的稿件类型: {postType}", post.PostType);
                        return false;
                    }

                    postMessage = await handler;
                }
                post.PublicMsgID = postMessage?.MessageId ?? -1;
            }
            else
            {
                var attachments = await _attachmentService.FetchAttachmentsByPostId(post.Id);
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
                    var warnMsg = await _botClient.SendTextMessageAsync(_channelService.AcceptChannel, warnText, allowSendingWithoutReply: true);
                    post.WarnTextID = warnMsg.MessageId;
                }

                var postMessages = await _botClient.SendMediaGroupAsync(_channelService.AcceptChannel, group);
                post.PublicMsgID = postMessages.First().MessageId;
                post.PublishMediaGroupID = postMessages.First().MediaGroupId ?? "";

                //记录媒体组消息
                await _mediaGroupService.AddPostMediaGroup(postMessages);
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
            }).ExecuteCommandAsync();
        }
        return true;
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

    public async Task<NewPosts?> GetLatestReviewingPostLink()
    {
        var now = DateTime.Now;
        var today = now.AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);

        var post = await Queryable().Where(x => x.CreateAt >= today && x.Status == EPostStatus.Reviewing).FirstAsync();
        return post;
    }

    public async Task<NewPosts?> GetPostByPostId(int postId)
    {
        return await Queryable().FirstAsync(x => x.Id == postId);
    }

    public Task<int> CountAllPosts()
    {
        return Queryable().Where(x => x.Status > EPostStatus.Cancel).CountAsync();
    }

    public Task<int> CountAllPosts(DateTime afterTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.Status > EPostStatus.Cancel).CountAsync();
    }

    public Task<int> CountAllPosts(DateTime afterTime, DateTime beforeTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.CreateAt < beforeTime && x.Status > EPostStatus.Cancel).CountAsync();
    }

    public Task<int> CountAcceptedPosts()
    {
        return Queryable().Where(x => x.Status == EPostStatus.Accepted).CountAsync();
    }

    public Task<int> CountAcceptedPosts(DateTime afterTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.Status == EPostStatus.Accepted).CountAsync();
    }

    public Task<int> CountAcceptedPosts(DateTime afterTime, DateTime beforeTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.CreateAt < beforeTime && x.Status == EPostStatus.Accepted).CountAsync();
    }

    public Task<int> CountAcceptedSecondPosts()
    {
        return Queryable().Where(x => x.Status == EPostStatus.AcceptedSecond).CountAsync();
    }

    public Task<int> CountAcceptedSecondPosts(DateTime afterTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.Status == EPostStatus.AcceptedSecond).CountAsync();
    }

    public Task<int> CountAcceptedSecondPosts(DateTime afterTime, DateTime beforeTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.CreateAt < beforeTime && x.Status == EPostStatus.AcceptedSecond).CountAsync();
    }

    public Task<int> CountRejectedPosts()
    {
        return Queryable().Where(x => x.Status == EPostStatus.Rejected).CountAsync();
    }

    public Task<int> CountRejectedPosts(DateTime afterTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.Status == EPostStatus.Rejected).CountAsync();
    }

    public Task<int> CountRejectedPosts(DateTime afterTime, DateTime beforeTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.CreateAt < beforeTime && x.Status == EPostStatus.Rejected).CountAsync();
    }

    public Task<int> CountExpiredPosts()
    {
        return Queryable().Where(x => x.Status < 0).CountAsync();
    }

    public Task<int> CountExpiredPosts(DateTime afterTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.Status < 0).CountAsync();
    }

    public Task<int> CountReviewingPosts(DateTime afterTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.Status == EPostStatus.Reviewing).CountAsync();
    }

    public Task<int> CountReviewingPosts(DateTime afterTime, DateTime beforeTime)
    {
        return Queryable().Where(x => x.CreateAt >= afterTime && x.CreateAt < beforeTime && x.Status == EPostStatus.Reviewing).CountAsync();
    }

    public Task RevocationPost(NewPosts post)
    {
        post.Status = EPostStatus.Revocation;
        post.ModifyAt = DateTime.Now;
        return Updateable(post).UpdateColumns(static x => new { x.Status, x.ModifyAt }).ExecuteCommandAsync();
    }

    public Task CancelPost(NewPosts post)
    {
        post.Status = EPostStatus.Cancel;
        post.ModifyAt = DateTime.Now;
        return Updateable(post).UpdateColumns(static x => new { x.Status, x.ModifyAt }).ExecuteCommandAsync();
    }

    public Task EditPostText(NewPosts post, string text)
    {
        post.Text = text;
        post.ModifyAt = DateTime.Now;
        return Updateable(post).UpdateColumns(static x => new { x.Text }).ExecuteCommandAsync();
    }

    public Task SetPostAnonymous(NewPosts post, bool anonymous)
    {
        post.Anonymous = anonymous;
        post.ModifyAt = DateTime.Now;
        return Updateable(post).UpdateColumns(static x => new { x.Anonymous, x.ModifyAt }).ExecuteCommandAsync();
    }

    public Task SetPostSpoiler(NewPosts post, bool spoiler)
    {
        post.HasSpoiler = spoiler;
        post.ModifyAt = DateTime.Now;
        return Updateable(post).UpdateColumns(static x => new { x.HasSpoiler, x.ModifyAt }).ExecuteCommandAsync();
    }

    public Task<bool> IfExistsMediaGroupId(string mediaGroupId)
    {
        return Queryable().AnyAsync(x => x.OriginMediaGroupID == mediaGroupId);
    }

    public async Task<NewPosts?> GetRandomPost()
    {
        return await Queryable()
                    .Where(static x => x.Status == EPostStatus.Accepted && x.PostType == MessageType.Photo)
                    .OrderBy(static x => SqlFunc.GetRandom()).FirstAsync();
    }

    public Task<NewPosts> GetInPlanPost()
    {
        return Queryable().Where(static x => x.Status == EPostStatus.InPlan).FirstAsync();
    }

    public Task UpdatePostStatus(NewPosts post, EPostStatus status)
    {
        post.Status = status;
        post.ModifyAt = DateTime.Now;
        return Updateable(post).UpdateColumns(static x => new { x.Status, x.ModifyAt }).ExecuteCommandAsync();
    }

    public Task<int> CreateNewPosts(NewPosts post)
    {
        return Insertable(post).ExecuteReturnIdentityAsync();
    }

    public Task<List<NewPosts>> GetExpiredPosts(DateTime beforeTime)
    {
        return Queryable()
            .Where(x => (x.Status == EPostStatus.Padding || x.Status == EPostStatus.Reviewing) && x.ModifyAt < beforeTime)
            .ToListAsync();
    }

    public Task<List<NewPosts>> GetExpiredPosts(long userID, DateTime beforeTime)
    {
        return Queryable()
            .Where(x => x.PosterUID == userID && (x.Status == EPostStatus.Padding || x.Status == EPostStatus.Reviewing) && x.ModifyAt < beforeTime)
            .ToListAsync();
    }
}
