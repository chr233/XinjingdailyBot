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
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler
{
    [AppService(ServiceType = typeof(IChannelPostHandler), ServiceLifetime = LifeTime.Singleton)]
    public class ChannelPostHandler : IChannelPostHandler
    {
        private readonly ILogger<ChannelPostHandler> _logger;
        private readonly IChannelService _channelService;
        private readonly ITelegramBotClient _botClient;

        public ChannelPostHandler(
            ILogger<ChannelPostHandler> logger,
            ITelegramBotClient botClient,
            IChannelService channelService)
        {
            _logger = logger;
            _botClient = botClient;
            _channelService = channelService;
        }

        public async Task OnTextChannelPostReceived(Users dbUser, Message message)
        {
            //if (!dbUser.Right.HasFlag(UserRights.SendPost))
            //{
            //    await _botClient.AutoReplyAsync(Langs.NoPostRight, message);
            //    return;
            //}
            //if (_channelService.ReviewGroup.Id == -1)
            //{
            //    await _botClient.AutoReplyAsync(Langs.ReviewGroupNotSet, message);
            //    return;
            //}

            //if (string.IsNullOrEmpty(message.Text))
            //{
            //    await _botClient.AutoReplyAsync(Langs.TextPostCantBeNull, message);
            //    return;
            //}

            //if (message.Text!.Length > MaxPostText)
            //{
            //    await _botClient.AutoReplyAsync($"文本长度超过上限 {MaxPostText}, 无法创建投稿", message);
            //    return;
            //}

            //ChannelOption channelOption = ChannelOption.Normal;
            //string? channelName = null, channelTitle = null;
            //if (message.ForwardFromChat?.Type == ChatType.Channel)
            //{
            //    long channelId = message.ForwardFromChat.Id;
            //    channelName = $"{message.ForwardFromChat.Username}/{message.ForwardFromMessageId}";
            //    channelTitle = message.ForwardFromChat.Title;
            //    channelOption = await _channelOptionService.FetchChannelOption(channelId, channelName, channelTitle);
            //}

            //BuildInTags tags = _textHelperService.FetchTags(message.Text);
            //string text = _textHelperService.ParseMessage(message);

            //bool anonymous = dbUser.PreferAnonymous;

            ////直接发布模式
            //bool directPost = dbUser.Right.HasFlag(UserRights.DirectPost);
            ////发送确认消息
            //var keyboard = directPost ? _markupHelperService.DirectPostKeyboard(anonymous, tags) : _markupHelperService.PostKeyboard(anonymous);
            //string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";

            ////生成数据库实体
            //Posts newPost = new()
            //{
            //    Anonymous = anonymous,
            //    Text = text,
            //    RawText = message.Text ?? "",
            //    ChannelName = channelName ?? "",
            //    ChannelTitle = channelTitle ?? "",
            //    Status = directPost ? PostStatus.Reviewing : PostStatus.Padding,
            //    PostType = message.Type,
            //    Tags = tags,
            //    PosterUID = dbUser.UserID
            //};

            ////套用频道设定
            //switch (channelOption)
            //{
            //    case ChannelOption.Normal:
            //        break;
            //    case ChannelOption.PurgeOrigin:
            //        postText += "\n由于系统设定, 来自该频道的投稿将不会显示来源";
            //        newPost.ChannelName += '~';
            //        break;
            //    case ChannelOption.AutoReject:
            //        postText = "由于系统设定, 暂不接受来自此频道的投稿";
            //        keyboard = null;
            //        newPost.Status = PostStatus.Rejected;
            //        break;
            //    default:
            //        _logger.LogError("未知的频道选项 {channelOption}", channelOption);
            //        return;
            //}

            //Message msg = await _botClient.SendTextMessageAsync(message.Chat.Id, postText, replyToMessageId: message.MessageId, replyMarkup: keyboard, allowSendingWithoutReply: true);

            ////修改数据库实体
            //newPost.OriginChatID = message.Chat.Id;
            //newPost.OriginMsgID = message.MessageId;
            //newPost.ActionMsgID = msg.MessageId;

            //if (directPost)
            //{
            //    newPost.ReviewMsgID = msg.MessageId;
            //    newPost.ManageMsgID = msg.MessageId;
            //}

            //await Insertable(newPost).ExecuteCommandAsync();
        }

        public async Task OnMediaChannelPostReceived(Users dbUser, Message message)
        {

        }

        /// <summary>
        /// mediaGroupID字典
        /// </summary>
        private ConcurrentDictionary<string, long> MediaGroupIDs { get; } = new();

        public async Task OnMediaGroupChannelPostReceived(Users dbUser, Message message)
        {

        }
    }
}
