using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Dispatcher;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Dispatcher;

[AppService(ServiceType = typeof(IUpdateHandler), ServiceLifetime = LifeTime.Scoped)]
public class UpdateDispatcherService : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateDispatcherService> _logger;
    private readonly IUserService _userService;
    private readonly IQueryDispatcherService _queryDispatcherService;
    private readonly IMessageDispatcherService _messageDispatcherService;

    public UpdateDispatcherService(
        ITelegramBotClient botClient,
        ILogger<UpdateDispatcherService> logger,
        IUserService userService,
        IQueryDispatcherService queryDispatcherService,
        IMessageDispatcherService messageDispatcherService)
    {
        _botClient = botClient;
        _logger = logger;
        _userService = userService;
        _queryDispatcherService = queryDispatcherService;
        _messageDispatcherService = messageDispatcherService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        var dbUser = await _userService.FetchUser(update);

        if (dbUser == null)
        {
            return;
        }

        var handler = update.Type switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            UpdateType.Message => _messageDispatcherService.OnMessageReceived(dbUser, update.Message!),
            UpdateType.EditedMessage => _messageDispatcherService.OnMessageReceived(dbUser, update.EditedMessage!),
            UpdateType.CallbackQuery => _queryDispatcherService.OnCallbackQueryReceived(dbUser, update.CallbackQuery!),
            //UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, dbUser, update.InlineQuery!),
            //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, dbUser, update.ChosenInlineResult!),
            _ => UnknownUpdateHandlerAsync(dbUser, update)
        };

        await handler;
    }

    private Task UnknownUpdateHandlerAsync(Users dbUser, Update update)
    {
        _logger.LogUpdate(update);
        return Task.CompletedTask;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}
