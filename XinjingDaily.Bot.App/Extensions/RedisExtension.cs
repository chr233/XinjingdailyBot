using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Net;
using System.Text.Json;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Infrastructure.Convertor;
using XinjingDaily.Bot.Infrastructure.Options;
using XinjingDaily.Bot.Service.HostedService.Init;

namespace XinjingDaily.Bot.WebAPI.Extensions;

/// <summary>
/// HttpClient扩展
/// </summary>
public static class RedisExtension
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 转换为有效的 EndPoint
    /// </summary>
    /// <param name="host">地址</param>
    /// <param name="port">端口</param>
    /// <param name="endPoint">输出转换后的 EndPoint（失败则为 null）</param>
    /// <returns>是否能成功转换</returns>
    private static bool TryConvertToEndPoint(string? host, int port, out EndPoint? endPoint)
    {
        endPoint = null;

        // 1. 空值/空白校验
        if (string.IsNullOrWhiteSpace(host))
        {
            return false;
        }

        try
        {
            // 4. 尝试解析为主机名/IP 地址，转换为 EndPoint
            // 优先尝试解析为 IP 地址（转 IPEndPoint）
            if (IPAddress.TryParse(host, out var ipAddress))
            {
                endPoint = new IPEndPoint(ipAddress, port);
            }
            else
            {
                // 解析为域名（转 DnsEndPoint）
                // 注：DnsEndPoint 不验证域名是否可解析，仅验证格式
                endPoint = new DnsEndPoint(host, port);
            }

            // 5. 额外校验：StackExchange.Redis 要求 EndPoint 必须是 IPEndPoint/DnsEndPoint
            if (endPoint is not IPEndPoint and not DnsEndPoint)
            {
                endPoint = null;
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            // 捕获转换过程中的异常（如端口溢出、主机名非法等）
            _logger.Warn($"转换 EndPoint 失败：{ex.Message}");
            endPoint = null;
            return false;
        }
    }

    /// <summary>
    /// 安全输出数据库配置（隐藏密码）
    /// </summary>
    /// <param name="config"></param>
    private static void PrintDatabaseConfig(RedisConfig config)
    {
#pragma warning disable CA1869 // 缓存并重用“JsonSerializerOptions”实例
        var options = new JsonSerializerOptions {
            WriteIndented = true,
        };
#pragma warning restore CA1869 // 缓存并重用“JsonSerializerOptions”实例
        options.Converters.Add(new RedisConfigJsonConverter());

        var json = JsonSerializer.Serialize(config, options);

        _logger.Warn("当前 Redis 配置: {0}", json);
    }

    extension(IServiceCollection services)
    {
        /// <summary>
        /// 注册HttpClient
        /// </summary>
        /// <param name="services"></param>
        public void AddRedis()
        {
            services.AddSingleton<IConnectionMultiplexer>(sp => {
                var config = sp.GetRequiredService<IOptions<AppSettings>>().Value.Redis;

                if (!TryConvertToEndPoint(config.Host, config.Port, out var endPoint) || endPoint == null)
                {
                    PrintDatabaseConfig(config);
                    _logger.Error("Redis 配置有误, 请检查 Redis 节配置");
                    _logger.Info("按任意键退出...");
                    Console.ReadKey();
                    Environment.Exit(1);
                }

                var options = new ConfigurationOptions {
                    // 1. 基础配置
                    DefaultDatabase = config.DefaultDatabase,
                    Password = config.Password,
                    ClientName = "XinjingDaily.Bot",
                    AllowAdmin = false,

                    // 2. 超时配置 (ms)
                    ConnectTimeout = config.ConnectTimeout,
                    SyncTimeout = config.SyncTimeout,
                    AsyncTimeout = config.AsyncTimeout,
                    ConnectRetry = config.ConnectRetry,

                    // 3. SSL 安全配置
                    Ssl = config.Ssl,
                    SslHost = config.SslHost,

                    // 5. 高可用与心跳
                    AbortOnConnectFail = true, // 关键：生产环境建议设为 false，即使初始连接失败也会继续重试
                    KeepAlive = config.KeepAlive,
                };

                options.EndPoints.Add(endPoint);

                return ConnectionMultiplexer.Connect(options);
            });

            services.AddHostedService<RedisInitHostedService>();
        }
    }
}
