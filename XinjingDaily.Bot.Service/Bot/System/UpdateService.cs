using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.Infrastructure.Extensions;
using XinjingDaily.Bot.Interface.Bot.System;

namespace XinjingDaily.Bot.Service.Bot;

[RegisterScoped]
public class UpdateService(
     ILogger<UpdateService> _logger,
     IDispatcherService _dispatcherService) : IUpdateService
{
    private int LastUpdateId { get; set; } = 0;

    /// <inheritdoc/>
    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        _logger.LogUpdate(update);

        if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.NewChatTitle)
        {
            //_channelService.OnChatTitleChanged(update.Message.Chat, update.Message.NewChatTitle);
        }

        var dbUser = new UserInfo();

        if (dbUser == null)
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
            UpdateType.ChannelPost => _dispatcherService.OnChannalPostReceived(dbUser, update.ChannelPost!),
            UpdateType.Message => _dispatcherService.OnMessageReceived(dbUser, update.Message!),
            UpdateType.CallbackQuery => _dispatcherService.OnCallbackQueryReceived(dbUser, update.CallbackQuery!),
            UpdateType.ChatJoinRequest => _dispatcherService.OnJoinRequestReceived(dbUser, update.ChatJoinRequest!),
            UpdateType.InlineQuery => _dispatcherService.OnInlineQueryReceived(dbUser, update.InlineQuery!),
            _ => _dispatcherService.OnOtherUpdateReceived(dbUser, update)
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
