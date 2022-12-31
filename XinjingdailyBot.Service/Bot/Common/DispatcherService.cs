using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Common
{
    [AppService(ServiceType = typeof(IDispatcherService), ServiceLifetime = LifeTime.Scoped)]
    internal class DispatcherService : IDispatcherService
    {
        private readonly IMessageHandler _messageHandler;
        private readonly ICommandHandler _commandHandler;
        private readonly IChannelPostHandler _channelPostHandler;
        private readonly IChannelService _channelService;

        public DispatcherService(
            IMessageHandler messageHandler,
            ICommandHandler commandHandler,
            IChannelPostHandler channelPostHandler,
            IChannelService channelService)
        {
            _messageHandler = messageHandler;
            _commandHandler = commandHandler;
            _channelPostHandler = channelPostHandler;
            _channelService = channelService;
        }

        /// <summary>
        /// 收到私聊或者群组消息消息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task OnMessageReceived(Users dbUser, Message message)
        {
            var handler = message.Type switch
            {
                MessageType.Text => message.Text!.StartsWith("/") ?
                    _commandHandler.OnCommandReceived(dbUser, message) :
                    _messageHandler.OnTextMessageReceived(dbUser, message),
                MessageType.Photo => _messageHandler.OnMediaMessageReceived(dbUser, message),
                MessageType.Audio => _messageHandler.OnMediaMessageReceived(dbUser, message),
                MessageType.Video => _messageHandler.OnMediaMessageReceived(dbUser, message),
                MessageType.Voice => _messageHandler.OnMediaMessageReceived(dbUser, message),
                MessageType.Document => _messageHandler.OnMediaMessageReceived(dbUser, message),
                MessageType.Sticker => _messageHandler.OnMediaMessageReceived(dbUser, message),
                _ => null,
            };

            if (handler != null)
            {
                await handler;
            }
        }

        /// <summary>
        /// 收到频道消息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task OnChannalPostReceived(Users dbUser, Message message)
        {
            //仅监听发布频道的消息
            if (message.Chat.Id == _channelService.AcceptChannel.Id)
            {
                var handler = message.Type switch
                {
                    MessageType.Text => _channelPostHandler.OnTextChannelPostReceived(dbUser, message),
                    MessageType.Photo => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
                    MessageType.Audio => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
                    MessageType.Video => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
                    MessageType.Voice => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
                    MessageType.Document => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
                    MessageType.Sticker => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
                    _ => null,
                };

                if (handler != null)
                {
                    await handler;
                }
            }
        }

        /// <summary>
        /// 收到CallbackQuery
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task OnCallbackQueryReceived(Users dbUser, CallbackQuery query)
        {
            await _commandHandler.OnQueryCommandReceived(dbUser, query);
        }
    }
}
