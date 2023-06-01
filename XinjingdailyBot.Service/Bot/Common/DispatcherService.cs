﻿using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Service.Bot.Common
{
    /// <inheritdoc cref="IDispatcherService"/>
    [AppService(typeof(IDispatcherService), LifeTime.Scoped)]
    internal class DispatcherService : IDispatcherService
    {
        private readonly ILogger<DispatcherService> _logger;
        private readonly IMessageHandler _messageHandler;
        private readonly ICommandHandler _commandHandler;
        private readonly IChannelPostHandler _channelPostHandler;
        private readonly IChannelService _channelService;
        private readonly ITelegramBotClient _botClient;
        private readonly IJoinRequestHandler _joinRequestHandler;
        private readonly IInlineQueryHandler _inlineQueryHandler;
        private readonly IDialogueService _dialogueService;
        private readonly TagRepository _tagRepository;

        public DispatcherService(
            ILogger<DispatcherService> logger,
            IMessageHandler messageHandler,
            ICommandHandler commandHandler,
            IChannelPostHandler channelPostHandler,
            IChannelService channelService,
            ITelegramBotClient botClient,
            IJoinRequestHandler joinRequestHandler,
            IInlineQueryHandler inlineQueryHandler,
            IDialogueService dialogueService,
            TagRepository tagRepository)
        {
            _logger = logger;
            _messageHandler = messageHandler;
            _commandHandler = commandHandler;
            _channelPostHandler = channelPostHandler;
            _channelService = channelService;
            _botClient = botClient;
            _joinRequestHandler = joinRequestHandler;
            _inlineQueryHandler = inlineQueryHandler;
            _dialogueService = dialogueService;
            _tagRepository = tagRepository;
        }

        /// <summary>
        /// 删除子频道中的NSFW消息以及取消置顶其他消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task UnPinMessage(Message message)
        {
            try
            {
                if (_tagRepository.IsWarnText(message.Text))
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
                _logger.LogError(ex, "取消置顶出错");
            }
        }

        public async Task OnMessageReceived(Users dbUser, Message message)
        {
            await _dialogueService.RecordMessage(message);

            if (dbUser.UserID == 777000 && (message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup))
            {
                if (message.Chat.Id == _channelService.SubGroup.Id || message.Chat.Id == _channelService.CommentGroup.Id)
                {
                    await UnPinMessage(message);
                    return;
                }
            }

            if (message.Type == MessageType.Text && message.Text!.StartsWith("/"))
            {
                //处理命令
                await _commandHandler.OnCommandReceived(dbUser, message);
            }
            else
            {
                //处理私聊投稿以及群聊消息
                var handler = message.Type == MessageType.Text ? _messageHandler.OnTextMessageReceived(dbUser, message) : _messageHandler.OnMediaMessageReceived(dbUser, message);

                if (handler != null)
                {
                    await handler;
                }
            }
        }

        public async Task OnChannalPostReceived(Users dbUser, Message message)
        {
            //仅监听发布频道的消息
            if (message.Chat.Id == _channelService.AcceptChannel.Id)
            {
                var handler = message.Type switch {
                    MessageType.Text => _channelPostHandler.OnTextChannelPostReceived(dbUser, message),
                    MessageType.Photo => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
                    MessageType.Audio => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
                    MessageType.Video => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
                    MessageType.Voice => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
                    MessageType.Document => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
                    MessageType.Animation => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
                    _ => null,
                };

                if (handler != null)
                {
                    await handler;
                }
            }
        }

        public async Task OnCallbackQueryReceived(Users dbUser, CallbackQuery query)
        {
            await _commandHandler.OnQueryCommandReceived(dbUser, query);
        }

        public async Task OnJoinRequestReceived(Users dbUser, ChatJoinRequest request)
        {
            if (_channelService.IsGroupMessage(request.Chat.Id))
            {
                await _joinRequestHandler.OnJoinRequestReceived(dbUser, request);
            }
        }

        public async Task OnInlineQueryReceived(Users dbUser, InlineQuery query)
        {
            await _inlineQueryHandler.OnInlineQueryReceived(dbUser, query);
        }

        public Task OnOtherUpdateReceived(Users dbUser, Update update)
        {
            _logger.LogInformation("收到未知消息类型的消息, [{type}]", update.Type);
            return Task.CompletedTask;
        }
    }
}
