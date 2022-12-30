using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler
{
    [AppService(ServiceType = typeof(IMessageHandler), ServiceLifetime = LifeTime.Scoped)]
    public class MessageHandler : IMessageHandler
    {
        private readonly ILogger<MessageHandler> _logger;
        private readonly ITelegramBotClient _botClient;
        private readonly IChannelService _channelService;
        private readonly ITextHelperService _textHelperService;
        private readonly IPostService _postService;
        private readonly OptionsSetting _optionsSetting;

        public MessageHandler(
            ILogger<MessageHandler> logger,
            ITelegramBotClient botClient,
            IChannelService channelService,
            ITextHelperService textHelperService,
            IPostService postService,
            IOptions<OptionsSetting> optionsSetting)
        {
            _logger = logger;
            _botClient = botClient;
            _channelService = channelService;
            _textHelperService = textHelperService;
            _postService = postService;
            _optionsSetting = optionsSetting.Value;
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task OnMessageReceived(Users dbUser, Message message)
        {
            var msgType = message.Type;
            var msgText = msgType == MessageType.Text ? message.Text! : "";

            var isCommand = msgText.StartsWith('/');

            //检查是否封禁, 封禁后仅能使用命令, 不响应其他消息
            if (dbUser.IsBan && !isCommand)
            {
                return;
            }

            var isMediaGroup = message.MediaGroupId != null;
            var isPrivateChat = message.Chat.Type == ChatType.Private;
            var isGroupChat = message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup;
            var isCommentGroup = isGroupChat && message.Chat.Id == _channelService.CommentGroup.Id;
            var isSubGroup = isGroupChat && message.Chat.Id == _channelService.SubGroup.Id;
            var isReviewGroup = isGroupChat && message.Chat.Id == _channelService.ReviewGroup.Id;
            var isConfigedGroup = isCommentGroup || isSubGroup || isReviewGroup;

            //尚未设置评论群或者讨论群时始终处理所有群组的消息
            if (_channelService.CommentGroup.Id == -1 || _channelService.SubGroup.Id == -1)
            {
                isConfigedGroup = isGroupChat;
            }

            //取消绑定子频道的消息置顶
            if (dbUser.UserID == 777000 && //Telegram
                (message.Chat.Id == _channelService.SubGroup.Id || message.Chat.Id == _channelService.CommentGroup.Id))
            {
                if (isSubGroup || isCommentGroup)
                {
                    try
                    {
                        if (_textHelperService.NSFWWrning == message.Text)
                        {
                            await _botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        }
                        else
                        {
                            await _botClient.UnpinChatMessageAsync(message.Chat.Id, message.MessageId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "取消置顶消息是发生错误");
                    }
                }
                return;
            }

            _logger.LogMessage(message);

            switch (message.Type)
            {
                case MessageType.Text when isPrivateChat:
                    await _postService.HandleTextPosts(dbUser, message);
                    break;
                case MessageType.Photo when isMediaGroup && isPrivateChat:
                case MessageType.Audio when isMediaGroup && isPrivateChat:
                case MessageType.Video when isMediaGroup && isPrivateChat:
                case MessageType.Document when isMediaGroup && isPrivateChat:
                    await _postService.HandleMediaGroupPosts(dbUser, message);
                    break;

                case MessageType.Photo when isPrivateChat:
                case MessageType.Audio when isPrivateChat:
                case MessageType.Video when isPrivateChat:
                case MessageType.Document when isPrivateChat:
                    await _postService.HandleMediaPosts(dbUser, message);
                    break;

                case MessageType.Text when isConfigedGroup && !dbUser.IsBot:
                    //await GroupHandler.HandlerGroupMessage(_botClient, dbUser, message);
                    break;

                case MessageType.Photo when !isPrivateChat:
                case MessageType.Audio when !isPrivateChat:
                case MessageType.Video when !isPrivateChat:
                case MessageType.Document when !isPrivateChat:
                case MessageType.Text when !isPrivateChat:
                    if (isGroupChat && !isConfigedGroup && _optionsSetting.Bot.AutoLeaveOtherGroup)
                    {
                        _logger.LogWarning("自动退出未设置的群组");
                        try
                        {
                            await _botClient.LeaveChatAsync(message.Chat.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "自动退出群组 {chatProfile} 失败", message.Chat.ChatProfile());
                        }
                    }
                    break;

                default:
                    if (isPrivateChat)
                    {
                        await _botClient.AutoReplyAsync("不支持的消息类型, 当前仅支持 文字/图片/视频/音频/文件 投稿", message);
                    }
                    break;
            }
        }

        /// <summary>
        /// 处理文本消息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task OnTextMessageReceived(Users dbUser, Message message)
        {
            if (dbUser.IsBan)
            {
                return;
            }

            if (message.Chat.Type == ChatType.Private)
            {
                await _postService.HandleTextPosts(dbUser, message);
            }
        }

        /// <summary>
        /// 处理非文本消息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task OnMediaMessageReceived(Users dbUser, Message message)
        {
            if (dbUser.IsBan)
            {
                return;
            }

            if (message.Chat.Type == ChatType.Private)
            {
                if (message.MediaGroupId != null)
                {
                    await _postService.HandleMediaGroupPosts(dbUser, message);
                }
                else
                {
                    await _postService.HandleMediaPosts(dbUser, message);
                }

            }
        }
    }
}
