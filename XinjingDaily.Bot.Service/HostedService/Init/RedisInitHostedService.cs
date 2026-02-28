using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using XinjingDaily.Bot.Infrastructure;

namespace XinjingDaily.Bot.Service.HostedService.Init;

/// <summary>
/// 消息接收服务
/// </summary>
/// <remarks>
/// 消息接收服务
/// </remarks>
/// <param name="_logger"></param>
public class RedisInitHostedService(
    ILogger<RedisInitHostedService> _logger,
    IOptions<AppSettings> _options,
    IConnectionMultiplexer _multiplexer) : BackgroundService
{
    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var config = _options.Value.Redis;

        try
        {
            if (!_multiplexer.IsConnected)
            {
                _logger.LogError("Redis 连接失败");
                return;
            }

            var db = _multiplexer.GetDatabase(config.DefaultDatabase);
            var ping = await db.PingAsync().ConfigureAwait(false);

            _logger.LogInformation("Redis 连接成功, Ping: {Ping} ms", ping.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis 连接出错");
        }
    }
}