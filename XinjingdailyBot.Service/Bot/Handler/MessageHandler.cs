using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler
{
    [AppService(ServiceType = typeof(IMessageHandler), ServiceLifetime = LifeTime.Scoped)]
    public class MessageHandler : IMessageHandler
    {
        private readonly IPostService _postService;
        private readonly IChannelService _channelService;

        public MessageHandler(
            IPostService postService,
            IChannelService channelService)
        {
            _postService = postService;
            _channelService = channelService;
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
            else
            {
                var msg = message.ReplyToMessage;
                if(msg != null)
                {
                    if(msg.From?.Id == _channelService.BotUser.Id)
                    {
                        if()
                    }
                }
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
