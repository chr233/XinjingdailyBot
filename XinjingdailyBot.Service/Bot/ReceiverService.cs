using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Bot;

namespace XinjingdailyBot.Service.Bot;

/// <summary>
/// An abstract class to compose Receiver Service and Update Handler classes
/// </summary>
/// <typeparam name="TUpdateHandler">Update Handler to use in Update Receiver</typeparam>

[AppService(ServiceType = typeof(IReceiverService), ServiceLifetime = LifeTime.Transient)]
public class ReceiverService : IReceiverService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUpdateHandler _updateHandlers;
    private readonly ILogger<ReceiverService> _logger;

    public ReceiverService(
        ITelegramBotClient botClient,
        IUpdateHandler updateHandler,
        ILogger<ReceiverService> logger)
    {
        _botClient = botClient;
        _updateHandlers = updateHandler;
        _logger = logger;
    }

    /// <summary>
    /// Start to service Updates with provided Update Handler class
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    public async Task ReceiveAsync(CancellationToken stoppingToken)
    {
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>(),
            //ThrowPendingUpdates = true,
        };

        var me = await _botClient.GetMeAsync(stoppingToken);
        _logger.LogInformation("Start receiving updates for {BotName}", me.Username ?? "My Awesome Bot");

        // Start receiving updates
        await _botClient.ReceiveAsync(
            updateHandler: _updateHandlers,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken);
    }
}
