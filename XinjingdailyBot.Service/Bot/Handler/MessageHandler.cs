using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
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

        public MessageHandler(
            IPostService postService,
            IGroupMessageHandler groupMessageHandler)
        {
            _postService = postService;
            _groupMessageHandler = groupMessageHandler;
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
                //case ChatType.Channel:
                //    await _postService.HandleTextPosts(dbUser, message);
                //    break;
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
