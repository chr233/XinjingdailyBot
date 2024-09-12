using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
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

        var bots = await _botRepository.QueryBotByName(null, null, 1, 100).ConfigureAwait(false);

        List<Task> tasks = [];

        foreach (var bot in bots)
        {
            _logger.LogInformation("{bot}", bot);

            var scope = _serviceProvider.CreateScope();
            //scope.ServiceProvider.
            //var pollingService = scope.ServiceProvider.GetRequiredService<PollingService>();

            var httpClient = _httpClientFactory.CreateClient("Telegram");
            var options = new TelegramBotClientOptions(bot.BotToken);
            var telegramBotClient = new TelegramBotClient(options, httpClient);

            var updateHandler = scope.ServiceProvider.GetRequiredService<UpdateHandler>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<PollingService>>();
            _botManagerService.AddBot(bot.Id, telegramBotClient);

            var pollingService = new PollingService(logger, telegramBotClient, updateHandler);
            tasks.Add(pollingService.DoWork(cancellationToken));

        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

}