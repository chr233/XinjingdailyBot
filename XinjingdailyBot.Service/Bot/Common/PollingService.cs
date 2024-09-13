using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Service.Bot.Common;

/// <summary>
/// 消息接收服务
/// </summary>
public sealed class PollingService(
        IServiceProvider _serviceProvider,
        ILogger<PollingService> _logger,
        IChannelService _channelService,
        ICommandHandler _commandHandler,
        GroupRepository _groupRepository,
        LevelRepository _levelRepository,
        TagRepository _tagRepository,
        RejectReasonRepository _rejectReasonRepository,
        ITelegramBotClient _botClient,
        IPostService _postService,
        IOptions<OptionsSetting> _options) : BackgroundService
{
    private static readonly char[] Separator = ['|'];

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var option = _options.Value.Post;
        if (!string.IsNullOrEmpty(option.PureWords))
        {
            _logger.LogWarning("PureWords 字段已启用, 请使用 PureWordsList 字段");
            foreach (var word in option.PureWords.Split(Separator, StringSplitOptions.RemoveEmptyEntries))
            {
                option.PureWordsList.Add(word);
            }
        }
        _postService.InitTtlTimer();

        _logger.LogDebug("注册可用命令");
        _commandHandler.InstallCommands();

        _logger.LogInformation("读取基础信息");
        await _channelService.InitChannelInfo().ConfigureAwait(false);

        _logger.LogInformation("读取群组和等级设定");
        await _groupRepository.InitGroupCache().ConfigureAwait(false);
        await _levelRepository.InitLevelCache().ConfigureAwait(false);
        await _tagRepository.InitPostTagCache().ConfigureAwait(false);
        await _rejectReasonRepository.InitRejectReasonCache().ConfigureAwait(false);

        _logger.LogInformation("开始运行 Bot");
        await DoWork(stoppingToken).ConfigureAwait(false);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var updateService = scope.ServiceProvider.GetRequiredService<IUpdateService>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var receiverOptions = new ReceiverOptions {
                    AllowedUpdates = [],
                    ThrowPendingUpdates = _options.Value.Bot.ThrowPendingUpdates,
                    Limit = 100,
                };

                _logger.LogInformation("接收服务运行中...");

                await _botClient.ReceiveAsync(
                    updateHandler: updateService.HandleUpdateAsync,
                    pollingErrorHandler: updateService.HandlePollingErrorAsync,
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
