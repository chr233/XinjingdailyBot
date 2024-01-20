using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Service.Bot.Handler;

/// <inheritdoc cref="IChannelPostHandler"/>
[AppService(typeof(IChannelPostHandler), LifeTime.Singleton)]
public sealed class ChannelPostHandler(
        ILogger<ChannelPostHandler> _logger,
        IPostService _postService,
        ITextHelperService _textHelperService,
        IAttachmentService _attachmentService,
        IUserService _userService,
        IChannelOptionService _channelOptionService,
        TagRepository _tagRepository,
        ITelegramBotClient _botClient,
        IMediaGroupService _mediaGroupService,
        IChannelService _channelService) : IChannelPostHandler
{
    /// <inheritdoc/>
    public async Task OnTextChannelPostReceived(Users dbUser, Message message)
    {
        if (string.IsNullOrEmpty(message.Text))
        {
            return;
        }
        if (message.Text.Length > IPostService.MaxPostText)
        {
            return;
        }

        var second = message.Chat.Id == _channelService.SecondChannel?.Id;

        long channelId = -1, channelMsgId = -1;
        if (message.ForwardFromChat?.Type == ChatType.Channel)
        {
            channelId = message.ForwardFromChat.Id;
            channelMsgId = message.ForwardFromMessageId ?? -1;

            var option = await _channelOptionService.FetchChannelOption(message.ForwardFromChat);

            if (option == EChannelOption.AutoReject)
            {
                await _botClient.DeleteMessageAsync(message.Chat, message.MessageId);
                _logger.LogInformation("删除消息 {msgid}", message.MessageId);
                return;
            }
        }

        int newTag = _tagRepository.FetchTags(message.Text);
        string text = _textHelperService.ParseMessage(message);

        //生成数据库实体
        var newPost = new NewPosts {
            OriginChatID = message.Chat.Id,
            OriginMsgID = message.MessageId,
            OriginActionChatID = 0,
            OriginActionMsgID = 0,
            ReviewChatID = 0,
            ReviewMsgID = 0,
            ReviewActionChatID = 0,
            ReviewActionMsgID = 0,
            PublicMsgID = message.MessageId,
            Anonymous = false,
            Text = text,
            RawText = message.Text ?? "",
            ChannelID = channelId,
            ChannelMsgID = channelMsgId,
            Status = !second ? EPostStatus.Accepted : EPostStatus.AcceptedSecond,
            PostType = message.Type,
            Tags = newTag,
            PosterUID = dbUser.UserID,
            ReviewerUID = dbUser.UserID
        };

        await _postService.CreateNewPosts(newPost);

        //增加通过数量
        dbUser.AcceptCount++;
        await _userService.UpdateUserPostCount(dbUser);
    }

    /// <inheritdoc/>
    public async Task OnMediaChannelPostReceived(Users dbUser, Message message)
    {
        var second = message.Chat.Id == _channelService.SecondChannel?.Id;

        long channelId = -1, channelMsgId = -1;
        if (message.ForwardFromChat?.Type == ChatType.Channel)
        {
            channelId = message.ForwardFromChat.Id;
            channelMsgId = message.ForwardFromMessageId ?? -1;
            var option = await _channelOptionService.FetchChannelOption(message.ForwardFromChat);

            if (option == EChannelOption.AutoReject)
            {
                await _botClient.DeleteMessageAsync(message.Chat, message.MessageId);
                _logger.LogInformation("删除消息 {msgid}", message.MessageId);
                return;
            }
        }

        var newTags = _tagRepository.FetchTags(message.Caption);
        string text = _textHelperService.ParseMessage(message);

        //生成数据库实体
        var newPost = new NewPosts {
            OriginChatID = message.Chat.Id,
            OriginMsgID = message.MessageId,
            OriginActionChatID = 0,
            OriginActionMsgID = 0,
            ReviewChatID = 0,
            ReviewMsgID = 0,
            ReviewActionChatID = 0,
            ReviewActionMsgID = 0,
            PublicMsgID = message.MessageId,
            Anonymous = false,
            Text = text,
            RawText = message.Text ?? "",
            ChannelID = channelId,
            ChannelMsgID = channelMsgId,
            Status = !second ? EPostStatus.Accepted : EPostStatus.AcceptedSecond,
            PostType = message.Type,
            Tags = newTags,
            HasSpoiler = message.HasMediaSpoiler ?? false,
            PosterUID = dbUser.UserID,
            ReviewerUID = dbUser.UserID
        };

        var postID = await _postService.CreateNewPosts(newPost);

        var attachment = _attachmentService.GenerateAttachment(message, postID);

        if (attachment != null)
        {
            await _attachmentService.CreateAttachment(attachment);
        }

        //增加通过数量
        dbUser.AcceptCount++;
        await _userService.UpdateUserPostCount(dbUser);
    }

    /// <summary>
    /// mediaGroupID字典
    /// </summary>
    private ConcurrentDictionary<string, long> MediaGroupIDs { get; } = new();

    /// <inheritdoc/>
    public async Task OnMediaGroupChannelPostReceived(Users dbUser, Message message)
    {
        string mediaGroupId = message.MediaGroupId!;
        if (!MediaGroupIDs.TryGetValue(mediaGroupId, out long postID)) //如果mediaGroupId不存在则创建新Post
        {
            var second = message.Chat.Id == _channelService.SecondChannel?.Id;

            MediaGroupIDs.TryAdd(mediaGroupId, -1);

            bool exists = await _postService.IfExistsMediaGroupId(mediaGroupId);
            if (!exists)
            {
                long channelId = -1, channelMsgId = -1;
                if (message.ForwardFromChat?.Type == ChatType.Channel)
                {
                    channelId = message.ForwardFromChat.Id;
                    channelMsgId = message.ForwardFromMessageId ?? -1;

                    var option = await _channelOptionService.FetchChannelOption(message.ForwardFromChat);

                    if (option == EChannelOption.AutoReject)
                    {
                        await _botClient.DeleteMessageAsync(message.Chat, message.MessageId);
                        _logger.LogInformation("删除消息 {msgid}", message.MessageId);
                        return;
                    }
                }

                int newTags = _tagRepository.FetchTags(message.Caption);
                string text = _textHelperService.ParseMessage(message);

                //生成数据库实体
                var newPost = new NewPosts {
                    OriginChatID = message.Chat.Id,
                    OriginMsgID = message.MessageId,
                    OriginActionChatID = 0,
                    OriginActionMsgID = 0,
                    ReviewChatID = 0,
                    ReviewMsgID = 0,
                    ReviewActionChatID = 0,
                    ReviewActionMsgID = 0,
                    PublicMsgID = message.MessageId,
                    PublishMediaGroupID = mediaGroupId,
                    Anonymous = false,
                    Text = text,
                    RawText = message.Text ?? "",
                    ChannelID = channelId,
                    ChannelMsgID = channelMsgId,
                    Status = !second ? EPostStatus.Accepted : EPostStatus.AcceptedSecond,
                    PostType = message.Type,
                    Tags = newTags,
                    HasSpoiler = message.HasMediaSpoiler ?? false,
                    PosterUID = dbUser.UserID,
                    ReviewerUID = dbUser.UserID
                };

                postID = await _postService.CreateNewPosts(newPost);

                MediaGroupIDs[mediaGroupId] = postID;

                //两秒后停止接收媒体组消息
                _ = Task.Run(async () => {
                    await Task.Delay(1500);
                    MediaGroupIDs.Remove(mediaGroupId, out _);

                    //增加通过数量
                    dbUser.AcceptCount++;
                    await _userService.UpdateUserPostCount(dbUser);
                });
            }
        }

        if (postID > 0)
        {
            //更新附件
            var attachment = _attachmentService.GenerateAttachment(message, postID);
            if (attachment != null)
            {
                await _attachmentService.CreateAttachment(attachment);
            }

            //记录媒体组
            await _mediaGroupService.AddPostMediaGroup(message);
        }
    }
}
