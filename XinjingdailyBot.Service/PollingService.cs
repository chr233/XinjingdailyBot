using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace XinjingdailyBot.Service;

public sealed class PollingService(
    ILogger<PollingService> _logger,
    ITelegramBotClient _botClient,
    IUpdateHandler _updateHandler)
{
    public async Task DoWork(CancellationToken stoppingToken)
    {
        // ToDo: we can inject ReceiverOptions through IOptions container
        var receiverOptions = new ReceiverOptions() {
            AllowedUpdates = [],
            DropPendingUpdates = true,
        };

        // Make sure we receive updates until Cancellation Requested,
        // no matter what errors our ReceiveAsync get
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var me = await _botClient.GetMeAsync(stoppingToken);
                _logger.LogInformation("Start receiving updates for {BotName}", me.Username ?? "My Awesome Bot");

                // Start receiving updates
                await _botClient.ReceiveAsync(
                    updateHandler: _updateHandler,
                    receiverOptions: receiverOptions,
                    cancellationToken: stoppingToken);
            }
            // Update Handler only captures exception inside update polling loop
            // We'll catch all other exceptions here
            // see: https://github.com/TelegramBots/Telegram.Bot/issues/1106
            catch (Exception ex)
            {
                _logger.LogError("Polling failed with exception: {Exception}", ex);

                // Cooldown if something goes wrong
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
