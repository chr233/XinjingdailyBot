using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.Interface.Bot;
using XinjingDaily.Bot.Interface.Bot.System;

namespace XinjingDaily.Bot.Service.Bot.System;

[RegisterScoped]
public class DispatcherService(
        ILogger<DispatcherService> _logger,
        ITelegramBotService _botClient) : IDispatcherService
{
    /// <inheritdoc/>
    private async Task UnPinMessage(Message message)
    {
        //try
        //{
        //    if (_tagRepository.IsWarnText(message.Text))
        //    {
        //        await _botClient.DeleteMessage(message.Chat.Id, message.MessageId).ConfigureAwait(false);
        //    }
        //    else
        //    {
        //        await _botClient.UnpinChatMessage(message.Chat.Id, message.MessageId).ConfigureAwait(false);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, "取消置顶出错");
        //}
    }

    /// <inheritdoc/>
    public async Task OnMessageReceived(UserInfos dbUser, Message message)
    {
        //await _dialogueService.RecordMessage(message).ConfigureAwait(false);

        //if (dbUser.UserID == 777000 && (message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup))
        //{
        //    if (_channelService.IsGroupMessage(message.Chat.Id))
        //    {
        //        await UnPinMessage(message).ConfigureAwait(false);
        //        return;
        //    }
        //}

        //if (message.Type == MessageType.Text && message.Text!.StartsWith('/'))
        //{
        //    //处理命令
        //    await _commandHandler.OnCommandReceived(dbUser, message).ConfigureAwait(false);
        //}
        //else
        //{
        //    //处理私聊投稿以及群聊消息
        //    var handler = message.Type == MessageType.Text ? _messageHandler.OnTextMessageReceived(dbUser, message) : _messageHandler.OnMediaMessageReceived(dbUser, message);

        //    if (handler != null)
        //    {
        //        await handler.ConfigureAwait(false);
        //    }
        //}

        await _botClient.AutoReply("test", message).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task OnChannalPostReceived(UserInfos dbUser, Message message)
    {
        //仅监听发布频道的消息
        //var chatId = message.Chat.Id;
        //if (_channelService.IsChannelMessage(chatId) && chatId != _channelService.RejectChannel.Id)
        //{
        //    var handler = message.Type switch {
        //        MessageType.Text => _channelPostHandler.OnTextChannelPostReceived(dbUser, message),
        //        MessageType.Photo => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
        //        MessageType.Audio => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
        //        MessageType.Video => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
        //        MessageType.Voice => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
        //        MessageType.Document => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
        //        MessageType.Animation => _channelPostHandler.OnMediaChannelPostReceived(dbUser, message),
        //        _ => null,
        //    };

        //    if (handler != null)
        //    {
        //        await handler.ConfigureAwait(false);
        //    }
        //}
    }

    /// <inheritdoc/>
    public async Task OnCallbackQueryReceived(UserInfos dbUser, CallbackQuery query)
    {
        //await _commandHandler.OnQueryCommandReceived(dbUser, query).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task OnJoinRequestReceived(UserInfos dbUser, ChatJoinRequest request)
    {
        //if (_channelService.IsGroupMessage(request.Chat.Id))
        //{
        //    await _joinRequestHandler.OnJoinRequestReceived(dbUser, request).ConfigureAwait(false);
        //}
    }

    /// <inheritdoc/>
    public async Task OnInlineQueryReceived(UserInfos dbUser, InlineQuery query)
    {
        //await _inlineQueryHandler.OnInlineQueryReceived(dbUser, query).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task OnOtherUpdateReceived(UserInfos dbUser, Update update)
    {
        _logger.LogInformation("收到未知消息类型的消息, [{type}]", update.Type);
        return Task.CompletedTask;
    }
}

