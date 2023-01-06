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
    [AppService(ServiceType = typeof(IMessageHandler), ServiceLifetime = LifeTime.Scoped)]
    public class MessageHandler : IMessageHandler
    {
        private readonly IPostService _postService;
        private readonly IGroupMessageHandler _groupMessageHandler;
        private readonly ITelegramBotClient _botClient;

        public MessageHandler(
            IPostService postService,
            IGroupMessageHandler groupMessageHandler,
            ITelegramBotClient botClient)
        {
            _postService = postService;
            _groupMessageHandler = groupMessageHandler;
            _botClient = botClient;
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

            switch (message.Chat.Type)
            {
                case ChatType.Private:
                    await _postService.HandleTextPosts(dbUser, message);
                    break;
                case ChatType.Group:
                case ChatType.Supergroup:
                    await _groupMessageHandler.OnGroupTextMessageReceived(dbUser, message);
                    break;
                default:
                    break;
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
                switch (message.Type)
                {
                    case MessageType.Photo:
                    case MessageType.Audio:
                    case MessageType.Video:
                    case MessageType.Voice:
                    case MessageType.Document:
                    case MessageType.Animation:
                        if (message.MediaGroupId != null)
                        {
                            await _postService.HandleMediaGroupPosts(dbUser, message);
                        }
                        else
                        {
                            await _postService.HandleMediaPosts(dbUser, message);
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
