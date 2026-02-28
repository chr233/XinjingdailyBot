using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Interface.Bot.System;

namespace XinjingDaily.Bot.Service.HostedService;


/// <summary>
/// 消息接收服务
/// </summary>
public class PollingService(
        IServiceProvider _serviceProvider,
        ILogger<PollingService> _logger,
        ITelegramBotClient _botClient,
        IOptions<AppSettings> options) : BackgroundService
{

    private readonly bool _throwPendingUpdates = options.Value.Bot.ThrowPendingUpdates;

    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("注册可用命令");
        //_commandHandler.InstallCommands();

        _logger.LogInformation("读取基础信息");
        //await _channelService.InitChannelInfo().ConfigureAwait(false);

        _logger.LogInformation("读取群组和等级设定");
        //await _groupRepository.InitGroupCache().ConfigureAwait(false);
        //await _levelRepository.InitLevelCache().ConfigureAwait(false);
        //await _tagRepository.InitPostTagCache().ConfigureAwait(false);
        //await _rejectReasonRepository.InitRejectReasonCache().ConfigureAwait(false);

        _logger.LogInformation("开始运行 Bot");
        await DoPolling(stoppingToken).ConfigureAwait(false);
    }

    private async Task DoPolling(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var updateService = scope.ServiceProvider.GetRequiredService<IUpdateService>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var receiverOptions = new ReceiverOptions {
                    AllowedUpdates = [],
                    DropPendingUpdates = _throwPendingUpdates,
                    Limit = 100,
                };

                _logger.LogInformation("接收服务运行中...");

                await _botClient.ReceiveAsync(
                    updateHandler: updateService.HandleUpdateAsync,
                    errorHandler: updateService.HandlePollingErrorAsync,
                    receiverOptions: receiverOptions,
                    cancellationToken: stoppingToken).ConfigureAwait(false);
            }
            catch (ApiRequestException ex)
            {
                _logger.LogError(ex, "Telegram API 调用出错");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "接收服务运行出错");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);
            }
        }
    }
}
