using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Bot;

namespace XinjingdailyBot.Service.Bot;

[AppService(ServiceType = typeof(IReceiverService), ServiceLifetime = LifeTime.Transient)]
public class ReceiverService : IReceiverService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUpdateHandler _updateHandlers;
    private readonly ILogger<ReceiverService> _logger;
    private readonly OptionsSetting _optionsSetting;

    public ReceiverService(
        ITelegramBotClient botClient,
        IUpdateHandler updateHandler,
        ILogger<ReceiverService> logger,
        IOptions<OptionsSetting> options)
    {
        _botClient = botClient;
        _updateHandlers = updateHandler;
        _logger = logger;
        _optionsSetting = options.Value;
    }

    public async Task ReceiveAsync(CancellationToken stoppingToken)
    {
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>(),
            ThrowPendingUpdates = _optionsSetting.Bot.ThrowPendingUpdates,
        };

        var me = await _botClient.GetMeAsync(stoppingToken);
        _logger.LogInformation("Start receiving updates for {BotName}", me.Username ?? "My Awesome Bot");

        await _botClient.ReceiveAsync(
            updateHandler: _updateHandlers,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken);
    }
}
