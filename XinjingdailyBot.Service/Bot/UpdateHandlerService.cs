using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;

namespace XinjingdailyBot.Service.Bot;

[AppService(ServiceType = typeof(IUpdateHandler), ServiceLifetime = LifeTime.Transient)]
public class UpdateHandlerService : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandlerService> _logger;
    private readonly IUserService _userService;

    public UpdateHandlerService(
        ITelegramBotClient botClient,
        ILogger<UpdateHandlerService> logger,
        IUserService userService)
    {
        _botClient = botClient;
        _logger = logger;
        _userService = userService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        var dbuser = await _userService.FetchUser(update);

        await _botClient.SendTextMessageAsync(update.Message.Chat, "Hello World!");

        //var handler = update switch
        //{
        //    // UpdateType.Unknown:
        //    //UpdateType.ChannelPost:
        //    // UpdateType.EditedChannelPost:
        //    // UpdateType.ShippingQuery:
        //    // UpdateType.PreCheckoutQuery:
        //    // UpdateType.Poll:
        //    { Message: { } message } => BotOnMessageReceived(message, cancellationToken),
        //    { EditedMessage: { } message } => BotOnMessageReceived(message, cancellationToken),
        //    { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
        //    { InlineQuery: { } inlineQuery } => BotOnInlineQueryReceived(inlineQuery, cancellationToken),
        //    { ChosenInlineResult: { } chosenInlineResult } => BotOnChosenInlineResultReceived(chosenInlineResult, cancellationToken),
        //    _ => UnknownUpdateHandlerAsync(update, cancellationToken)
        //};

        //await handler;
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
