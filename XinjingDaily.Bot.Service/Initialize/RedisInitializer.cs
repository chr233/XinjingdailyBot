using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Infrastructure.Exceptions;
using XinjingDaily.Bot.Interface.InitService;

namespace XinjingDaily.Bot.Service.InitService;

/// <summary>
/// Redis初始化服务
/// </summary>
/// <param name="_logger"></param>
public class RedisInitializer(
    ILogger<RedisInitializer> _logger,
    IOptions<AppSettings> _options,
    IConnectionMultiplexer _multiplexer) : IInitializer
{
    /// <inheritdoc/>
    public int Order => 2;
    /// <inheritdoc/>
    public string Name => nameof(RedisInitializer);

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        var config = _options.Value.Redis;

        const int maxAttempts = 3;
        const int delayMs = 1000;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
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
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis 连接在第 {Attempt} 次尝试时出错", attempt);
                await Task.Delay(delayMs).ConfigureAwait(false);
            }

        }

        _logger.LogError("Redis 连接在 {MaxAttempts} 次尝试后失败", maxAttempts);
        throw new InitializeException("Redis 初始化失败, 请检查配置");
    }
}