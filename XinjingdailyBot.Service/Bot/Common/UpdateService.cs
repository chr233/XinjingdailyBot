using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;

namespace XinjingdailyBot.Service.Bot.Common;

/// <inheritdoc cref="IUpdateService"/>
[AppService(typeof(IUpdateService), LifeTime.Scoped)]
public sealed class UpdateService(
     ILogger<UpdateService> _logger,
     IUserService _userService,
     IDispatcherService _dispatcherService,
     IChannelService _channelService) : IUpdateService
{
    private int LastUpdateId { get; set; } = 0;

    /// <inheritdoc/>
    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        _logger.LogUpdate(update);

        if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.ChatTitleChanged)
        {
            _channelService.OnChatTitleChanged(update.Message.Chat, update.Message.NewChatTitle);
        }

        var dbUser = await _userService.FetchUserFromUpdate(update).ConfigureAwait(false);

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
