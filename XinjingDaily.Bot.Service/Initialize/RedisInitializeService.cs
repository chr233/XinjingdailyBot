using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Interface.InitService;

namespace XinjingDaily.Bot.Service.InitService;

/// <summary>
/// Redis初始化服务
/// </summary>
/// <param name="_logger"></param>
public class RedisInitializeService(
    ILogger<RedisInitializeService> _logger,
    IOptions<AppSettings> _options,
    IConnectionMultiplexer _multiplexer) : IInitializeService
{
    public int Order => 2;

    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> InitializeAsync(CancellationToken cancellationToken)
    {
        var config = _options.Value.Redis;

        const int maxAttempts = 3;
        const int delayMs = 1000; // 1 second delay between attempts

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Redis 初始化取消");
                return false;
            }

            try
            {
                if (!_multiplexer.IsConnected)
                {
                    _logger.LogWarning("Redis 未连接，尝试第 {Attempt}/{MaxAttempts}", attempt, maxAttempts);
                }
                else
                {
                    var db = _multiplexer.GetDatabase(config.DefaultDatabase);
                    var ping = await db.PingAsync().ConfigureAwait(false);

                    _logger.LogInformation("Redis 连接成功, Ping: {Ping} ms", ping.TotalMilliseconds);
                    return true;
                }
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(ex, "Redis 连接在第 {Attempt} 次尝试时出错，稍后重试", attempt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis 连接出错");
                return false;
            }

            if (attempt < maxAttempts)
            {
                try
                {
                    await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
            }
        }

        _logger.LogError("Redis 连接在 {MaxAttempts} 次尝试后失败", maxAttempts);
        return false;
    }
}