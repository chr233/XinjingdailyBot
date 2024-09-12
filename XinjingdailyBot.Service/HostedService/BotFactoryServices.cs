using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using XinjingdailyBot.Repository.Services;
using XinjingdailyBot.Service.Bot;
using XinjingdailyBot.Service.Common;

namespace XinjingdailyBot.Service.HostedService;

public sealed class BotFactoryServices(
    ILogger<BotFactoryServices> _logger,
    UpdateHandler _updateHandler,
    IHttpClientFactory _httpClientFactory,
    BotManagerService _botManagerService,
    BotRepository _botRepository
   ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MyBackgroundService is starting.");

        var bots = await _botRepository.QueryBotsEnabled().ConfigureAwait(false);

        List<Task> tasks = [];

        foreach (var bot in bots)
        {
            var httpClient = _httpClientFactory.CreateClient("Telegram");
            var options = new TelegramBotClientOptions(bot.BotToken);
            var telegramBotClient = new TelegramBotClient(options, httpClient, cancellationToken);

            _botManagerService.AddBot(bot.Id, telegramBotClient);

            var receiverOptions = new ReceiverOptions() {
                AllowedUpdates = [],
                DropPendingUpdates = true,
            };

            try
            {
                var me = await telegramBotClient.GetMeAsync(cancellationToken).ConfigureAwait(false);

                bot.UserId = me.Id;
                bot.Username = me.Username;
                bot.Firstname = me.FirstName;

                await _botRepository.UpdateBot(bot).ConfigureAwait(false);

                tasks.Add(telegramBotClient.ReceiveAsync(
                    updateHandler: _updateHandler,
                    receiverOptions: receiverOptions,
                    cancellationToken: cancellationToken));

            }
            catch (Exception ex)
            {
                _logger.LogError("登陆失败");
            }

            //tasks.Add(pollingService.StartAsync(cancellationToken));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

}
