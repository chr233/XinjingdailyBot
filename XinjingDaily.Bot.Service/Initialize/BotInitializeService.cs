using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Interface.InitService;

namespace XinjingDaily.Bot.Service.InitService;

/// <summary>
/// 机器人初始化服务
/// </summary>
/// <param name="_logger"></param>
public class BotInitializeService(
    ILogger<BotInitializeService> _logger,
    IOptions<AppSettings> _options,
    IConnectionMultiplexer _multiplexer) : IInitializeService
{
    public int Order => 10;

    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> InitializeAsync(CancellationToken cancellationToken)
    {
        var config = _options.Value.Redis;

        //try
        //{
        //    if (!_multiplexer.IsConnected)
        //    {
        //        _logger.LogError("Redis 连接失败");
        //        return false;
        //    }

        //    var db = _multiplexer.GetDatabase(config.DefaultDatabase);
        //    var ping = await db.PingAsync().ConfigureAwait(false);

        //    _logger.LogInformation("Redis 连接成功, Ping: {Ping} ms", ping.TotalMilliseconds);
        //    return true;
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, "Redis 连接出错");
        //    return false;
        //}

        return true;
    }
}