namespace XinjingDaily.Bot.Infrastructure.Options;

/// <summary>
/// Redis配置类
/// </summary>
public sealed record RedisConfig
{
    /// <summary>
    /// 主机
    /// </summary>
    public string? Host { get; init; }
    /// <summary>
    /// 端口
    /// </summary>
    public int Port { get; init; } = 6379;
    /// <summary>
    /// 连接密码 (可空)
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// 默认数据库, 默认为3, 0-15
    /// </summary>
    public int DefaultDatabase { get; set; } = 3;

    public bool Ssl { get; set; }
    public string? SslHost { get; set; }

    public string? KeyPrefix { get; init; } = "xjb";

    public int ConnectTimeout { get; set; } = 5000;
    public int SyncTimeout { get; set; } = 5000;
    public int AsyncTimeout { get; set; } = 5000;
    public int ConnectRetry { get; set; } = 5;

    public int KeepAlive { get; set; } = 60;
}
