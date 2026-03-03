using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingDaily.Bot.Infrastructure.Extensions;
using XinjingDaily.Bot.Interface.Bot.Storage;
using XinjingDaily.Bot.Interface.Bot.System;

namespace XinjingDaily.Bot.Service.Bot;

[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class UpdateService(
     ILogger<UpdateService> _logger,
     IUserService _userService,
     IChatService _chatService,
     IDispatcherService _dispatcherService) : IUpdateService
{
    private int LastUpdateId { get; set; } = 0;

    /// <inheritdoc/>
    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        _logger.LogUpdate(update);

        if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.NewChatTitle)
        {
            var message = update.Message!;
            await _chatService.OnNewChatTitle(message.Chat, message.NewChatTitle).ConfigureAwait(false);
            //_channelService.OnChatTitleChanged(update.Message.Chat, update.Message.NewChatTitle);
        }

        var userInfo = await _userService.QueryUserFromUpdate(update).ConfigureAwait(false);

        if (userInfo == null)
        {
            _logger.LogWarning("User not found in database");
            return;
        }

        if (LastUpdateId == update.Id)
        {
            _logger.LogWarning("检测到处理重复的 Update 跳过执行 {update}", update);
            return;
        }

        LastUpdateId = update.Id;

        var handler = update.Type switch {
            UpdateType.ChannelPost => _dispatcherService.OnChannelPostReceived(userInfo, update.ChannelPost!),
            UpdateType.Message => _dispatcherService.OnMessageReceived(userInfo, update.Message!),
            UpdateType.CallbackQuery => _dispatcherService.OnCallbackQueryReceived(userInfo, update.CallbackQuery!),
            UpdateType.ChatJoinRequest => _dispatcherService.OnJoinRequestReceived(userInfo, update.ChatJoinRequest!),
            UpdateType.InlineQuery => _dispatcherService.OnInlineQueryReceived(userInfo, update.InlineQuery!),
            _ => _dispatcherService.OnOtherUpdateReceived(userInfo, update)
        };

        if (handler != null)
        {
            try
            {
                await handler.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理轮询出错 {update}", update);
            }
        }
    }

    /// <inheritdoc/>
    public async Task HandlePollingErrorAsync(ITelegramBotClient _, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "处理轮询出错");

        if (exception is RequestException)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken).ConfigureAwait(false);
        }
    }
}
