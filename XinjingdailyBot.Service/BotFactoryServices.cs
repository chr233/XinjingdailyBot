using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Repository.Services;

namespace XinjingdailyBot.Service;

public sealed class BotFactoryServices(
    ILogger<BotFactoryServices> _logger,
    IServiceProvider _serviceProvider,
    IHttpClientFactory _httpClientFactory,
    BotManagerService _botManagerService,
    BotRepository _botRepository,
    UserRepository _userRepository
   ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MyBackgroundService is starting.");

        var bots = await _botRepository.QueryBotsEnabled().ConfigureAwait(false);

        List<Task> tasks = [];

        foreach (var bot in bots)
        {
            _logger.LogInformation("{bot}", bot);

            var scope = _serviceProvider.CreateScope();
            //scope.ServiceProvider.
            //var pollingService = scope.ServiceProvider.GetRequiredService<PollingService>();

            var httpClient = _httpClientFactory.CreateClient("Telegram");
            var options = new TelegramBotClientOptions(bot.BotToken);
            var telegramBotClient = new TelegramBotClient(options, httpClient, cancellationToken);

            var updateHandler = scope.ServiceProvider.GetRequiredService<UpdateHandler>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<BotWorkerService>>();
            _botManagerService.AddBot(bot.Id, telegramBotClient);

            //var pollingService = new BotWorkerService(logger, telegramBotClient, updateHandler);

            var receiverOptions = new ReceiverOptions() {
                AllowedUpdates = [],
                DropPendingUpdates = true,
            };


            try
            {
                var me = await telegramBotClient.GetMeAsync(cancellationToken).ConfigureAwait(false);

                bot.UserId = me.Id;
                bot.Username = me.Username;
                bot.Nickname = me.FullName();

                await _botRepository.UpdateBot(bot).ConfigureAwait(false);

                tasks.Add(telegramBotClient.ReceiveAsync(
                    updateHandler: updateHandler,
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
