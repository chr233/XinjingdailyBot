using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using XinjingDaily.Bot.Infrastructure.Bot.Context;
using XinjingDaily.Bot.Infrastructure.Configs;
using XinjingDaily.Bot.IRepository.Context;

namespace XinjingDaily.Bot.Repository.Context;

[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class ContextRedisRepository : IContextRedisRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<ContextRedisRepository> _logger;
    private readonly TimeSpan _ttl;
    private readonly string _prefix;

    public ContextRedisRepository(
        IConnectionMultiplexer redis,
        IOptions<AppSettings> options,
        ILogger<ContextRedisRepository> logger)
    {
        _redis = redis;
        _logger = logger;
        var cfg = options.Value.Context ?? new ContextConfig();
        _ttl = TimeSpan.FromSeconds(cfg.TtlSeconds);
        _prefix = options.Value.RedisPrefix ?? "xjbot";
    }

    private string UserKey(int userId, long chatId) => $"{_prefix}:ctx:u:{userId}:{chatId}";
    private string ChatKey(string command, long chatId) => $"{_prefix}:ctx:c:{command}:{chatId}";

    public async Task<ContextRedisDto?> GetUserContextAsync(int userId, long chatId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var val = await db.StringGetAsync(UserKey(userId, chatId)).ConfigureAwait(false);
            if (val.IsNullOrEmpty) return null;
            return JsonSerializer.Deserialize<ContextRedisDto>(val.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Context] Redis 读取 user_context 失败 u={U} c={C}", userId, chatId);
            return null;
        }
    }

    public async Task<bool> SetUserContextAsync(int userId, long chatId, ContextRedisDto dto)
    {
        try
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(dto);
            return await db.StringSetAsync(UserKey(userId, chatId), json, _ttl).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Context] Redis 写入 user_context 失败 u={U} c={C}", userId, chatId);
            return false;
        }
    }

    public async Task<ContextRedisDto?> GetChatContextAsync(string command, long chatId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var val = await db.StringGetAsync(ChatKey(command, chatId)).ConfigureAwait(false);
            if (val.IsNullOrEmpty) return null;
            return JsonSerializer.Deserialize<ContextRedisDto>(val.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Context] Redis 读取 chat_context 失败 cmd={Cmd} c={C}", command, chatId);
            return null;
        }
    }

    public async Task<bool> SetChatContextAsync(string command, long chatId, ContextRedisDto dto)
    {
        try
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(dto);
            return await db.StringSetAsync(ChatKey(command, chatId), json, _ttl).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Context] Redis 写入 chat_context 失败 cmd={Cmd} c={C}", command, chatId);
            return false;
        }
    }
}
