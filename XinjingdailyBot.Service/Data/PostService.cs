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
    [AppService(ServiceType = typeof(IPostService), ServiceLifetime = LifeTime.Transient)]
    public sealed class PostService : BaseService<Posts>, IPostService
    {
        private readonly ILogger<PostService> _logger;
        private readonly PostRepository _postRepository;
        private readonly AttachmentRepository _attachmentRepository;
        private readonly IChannelService _channelService;
        private readonly IChannelOptionService _channelOptionService;
        private readonly ITextHelperService _textHelperService;
        private readonly IMarkupHelperService _markupHelperService;
        private readonly IAttachmentHelperService _attachmentHelperService;

        /// <summary>
        /// 文字投稿长度上限
        /// </summary>
        private static readonly int MaxPostText = 2000;

        public PostService(
            ILogger<PostService> logger,
            PostRepository postRepository,
            AttachmentRepository attachmentRepository,
            IChannelService channelService,
            IChannelOptionService channelOptionService,
            ITextHelperService textHelperService,
            IMarkupHelperService markupHelperService,
            IAttachmentHelperService attachmentHelperService)
        {
            _logger = logger;
            _postRepository = postRepository;
            _attachmentRepository = attachmentRepository;
            _channelService = channelService;
            _channelOptionService = channelOptionService;
            _textHelperService = textHelperService;
            _markupHelperService = markupHelperService;
            _attachmentHelperService = attachmentHelperService;
        }

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

            bool anymouse = dbUser.PreferAnymouse;

            //直接发布模式
            bool directPost = dbUser.Right.HasFlag(UserRights.DirectPost);
            //发送确认消息
            var keyboard = directPost ? _markupHelperService.DirectPostKeyboard(anymouse, tags) : _markupHelperService.PostKeyboard(anymouse);
            string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";

            //生成数据库实体
            Posts newPost = new()
            {
                Anymouse = anymouse,
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

            await _postRepository.Insertable(newPost).ExecuteCommandAsync();
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

            bool anymouse = dbUser.PreferAnymouse;

            //直接发布模式
            bool directPost = dbUser.Right.HasFlag(UserRights.DirectPost);
            //发送确认消息
            var keyboard = directPost ? _markupHelperService.DirectPostKeyboard(anymouse, tags) : _markupHelperService.PostKeyboard(anymouse);
            string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";

            //生成数据库实体
            Posts newPost = new()
            {
                Anymouse = anymouse,
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

            long postID = await _postRepository.Insertable(newPost).ExecuteReturnBigIdentityAsync();

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

                bool exists = await _postRepository.Queryable().AnyAsync(x => x.MediaGroupID == mediaGroupId);
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

                    bool anymouse = dbUser.PreferAnymouse;

                    //直接发布模式
                    bool directPost = dbUser.Right.HasFlag(UserRights.DirectPost);

                    //发送确认消息
                    var keyboard = directPost ? _markupHelperService.DirectPostKeyboard(anymouse, tags) : _markupHelperService.PostKeyboard(anymouse);
                    string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";

                    Message msg = await botClient.SendTextMessageAsync(message.Chat.Id, "处理中, 请稍后", replyToMessageId: message.MessageId, allowSendingWithoutReply: true);

                    //生成数据库实体
                    Posts newPost = new()
                    {
                        OriginChatID = message.Chat.Id,
                        OriginMsgID = message.MessageId,
                        ActionMsgID = msg.MessageId,
                        Anymouse = anymouse,
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

                    postID = await _postRepository.Insertable(newPost).ExecuteReturnBigIdentityAsync();

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
    }
}
