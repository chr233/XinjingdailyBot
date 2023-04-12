using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler
{
    [AppService(typeof(IMessageHandler), LifeTime.Singleton)]
    public class MessageHandler : IMessageHandler
    {
        private readonly IPostService _postService;
        private readonly IGroupMessageHandler _groupMessageHandler;
        private readonly ITelegramBotClient _botClient;
        private readonly IForwardMessageHandler _forwardMessageHandler;

        public MessageHandler(
            IPostService postService,
            IGroupMessageHandler groupMessageHandler,
            ITelegramBotClient botClient,
            IForwardMessageHandler forwardMessageHandler)
        {
            _postService = postService;
            _groupMessageHandler = groupMessageHandler;
            _botClient = botClient;
            _forwardMessageHandler = forwardMessageHandler;
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
                if (message.ForwardFromChat != null)
                {
                    var handled = await _forwardMessageHandler.OnForwardMessageReceived(dbUser, message);
                    if (handled)
                    {
                        return;
                    }
                }
                if (await _postService.CheckPostLimit(dbUser, message, null))
                {
                    await _postService.HandleTextPosts(dbUser, message);
                }
            }
            else if (message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup)
            {
                await _groupMessageHandler.OnGroupTextMessageReceived(dbUser, message);
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
                if (message.ForwardFromChat != null)
                {
                    var handled = await _forwardMessageHandler.OnForwardMessageReceived(dbUser, message);
                    if (!handled)
                    {
                        return;
                    }
                }
                switch (message.Type)
                {
                    case MessageType.Photo:
                    case MessageType.Audio:
                    case MessageType.Video:
                    case MessageType.Voice:
                    case MessageType.Document:
                    case MessageType.Animation:
                        if (await _postService.CheckPostLimit(dbUser, message, null))
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

                        break;
                    default:
                        await _botClient.AutoReplyAsync($"暂不支持的投稿类型 {message.Type}", message);
                        break;
                }
            }
        }
    }
}
