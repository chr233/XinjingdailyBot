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

[AppService(ServiceType = typeof(IUpdateService), ServiceLifetime = LifeTime.Scoped)]
public class UpdateService : IUpdateService
{
    private readonly ILogger<UpdateService> _logger;
    private readonly IUserService _userService;
    private readonly IDispatcherService _dispatcherService;

    public UpdateService(
        ILogger<UpdateService> logger,
        IUserService userService,
       IDispatcherService dispatcherService)
    {
        _logger = logger;
        _userService = userService;
        _dispatcherService = dispatcherService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        var dbUser = await _userService.FetchUserFromUpdate(update);

        if (dbUser == null)
        {
            return;
        }

        _logger.LogUpdate(update);

        var handler = update.Type switch
        {
            // UpdateType.Unknown:
            UpdateType.ChannelPost => _dispatcherService.OnMessageReceived(dbUser, update.ChannelPost!),
            UpdateType.EditedChannelPost => _dispatcherService.OnMessageReceived(dbUser, update.EditedChannelPost!),
            UpdateType.Message => _dispatcherService.OnMessageReceived(dbUser, update.Message!),
            UpdateType.EditedMessage => _dispatcherService.OnMessageReceived(dbUser, update.EditedMessage!),
            UpdateType.CallbackQuery => _dispatcherService.OnCallbackQueryReceived(dbUser, update.CallbackQuery!),
            //UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, dbUser, update.InlineQuery!),
            //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, dbUser, update.ChosenInlineResult!),
            _ => null
        };

        if (handler != null)
        {
            await handler;
        }
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("处理轮询出错: {ErrorMessage}", ErrorMessage);

        if (exception is RequestException)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }
}
