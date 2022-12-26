using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Bot.Handler;

namespace XinjingdailyBot.Service.Bot.Common;


[AppService(ServiceLifetime = LifeTime.Transient)]
public class PollingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PollingService> _logger;
    private readonly IChannelService _channelService;
    private readonly ICommandHandler _commandHandler;

    public PollingService(
        IServiceProvider serviceProvider,
        ILogger<PollingService> logger,
        IChannelService channelService,
        ICommandHandler commandHandler)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _channelService = channelService;
        _commandHandler = commandHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("注册可用命令");
        await _commandHandler.InitCommands();

        _logger.LogInformation("读取基础信息");
        await _channelService.InitChannelInfo();

        _logger.LogInformation("开始运行 Bot");
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var receiver = scope.ServiceProvider.GetRequiredService<IReceiverService>();

                await receiver.ReceiveAsync(stoppingToken);
            }

            catch (Exception ex)
            {
                _logger.LogError("接收服务运行出错: {Exception}", ex);

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
