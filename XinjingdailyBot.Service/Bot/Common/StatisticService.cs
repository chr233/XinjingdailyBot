using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Interface.Helper;

namespace XinjingdailyBot.Service.Bot.Common;

/// <summary>
/// 统计信息服务
/// </summary>
public sealed class StatisticService(
    IOptions<OptionsSetting> _options,
    IHttpHelperService _httpHelperService) : BackgroundService, IDisposable
{
    /// <summary>
    /// 统计Timer
    /// </summary>
    private Timer? StatisticTimer { get; set; }

    /// <inheritdoc/>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.Value.Statistic)
        {
            StatisticTimer = new Timer(
                async (_) => await _httpHelperService.SendStatistic(),
                null,
                TimeSpan.FromMinutes(30),
                TimeSpan.FromHours(24)
            );
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose() => StatisticTimer?.Dispose();
}
