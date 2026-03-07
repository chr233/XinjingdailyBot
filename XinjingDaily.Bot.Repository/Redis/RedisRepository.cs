using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Infrastructure.Extensions;
using XinjingDaily.Bot.IRepository.Redis;

namespace XinjingDaily.Bot.Repository.Redis;

[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public sealed class RedisRepository : IRedisRepository
{
    private readonly ILogger<RedisRepository> _logger;
    private readonly IDatabase _db;
    private readonly string _keyPrefix;
    private readonly bool _enableLog;

    public RedisRepository(
        IConnectionMultiplexer multiplexer,
        IOptions<AppSettings> options,
        ILogger<RedisRepository> logger)
    {
        _logger = logger;
        _db = multiplexer.GetDatabase(options.Value.Redis.DefaultDatabase);
        _keyPrefix = options.Value.Redis.KeyPrefix ?? string.Empty;
        _enableLog = options.Value.Redis.LogRedis;
    }

    private string GetFullKey(string key)
    {
        return string.IsNullOrEmpty(_keyPrefix) ? key : $"{_keyPrefix}:{key}";
    }

    #region String 类型操作
    /// <inheritdoc />
    public async Task<string?> GetStringAsync(string key)
    {
        var fullKey = GetFullKey(key);
        var value = await _db.StringGetAsync(fullKey).ConfigureAwait(false);

        if (_enableLog)
        {
            _logger.LogDebug("GetString: {Key} = {Value}", fullKey, value);
        }

        return value.HasValue ? value.ToString() : null;
    }

    /// <inheritdoc />
    public async Task<bool> SetStringAsync(string key, string value, int? expirySeconds = null, When when = When.Always)
    {
        var expiry = TimeSpan.FromSeconds(expirySeconds ?? 0);
        return await SetStringAsync(key, value, expiry, when).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null, When when = When.Always)
    {
        var fullKey = GetFullKey(key);
        var result = await _db.StringSetAsync(fullKey, value, expiry, when).ConfigureAwait(false);
        if (_enableLog)
        {
            _logger.LogDebug("SetString {Key} = {Value} | expiry: {Expiry}", fullKey, result, expiry);
        }
        return result;
    }

    #endregion

    #region 泛型操作

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var fullKey = GetFullKey(key);
        var value = await _db.StringGetAsync(fullKey).ConfigureAwait(false);
        if (!value.HasValue)
        {
            return null;
        }

        try
        {
            var json = value.ToString();
            if (_enableLog)
            {
                _logger.LogDebug("Get<T> key: {Key} = {Value}", fullKey, json);
            }

            return json.ToJsonObject<T>();
        }
        catch (JsonException ex)
        {
            if (_enableLog)
            {
                _logger.LogWarning(ex, "Get<T> key: {Key} failed to deserialize value", fullKey);
            }
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SetAsync<T>(string key, T value, int? expirySeconds = null, When when = When.Always) where T : class
    {
        var expiry = TimeSpan.FromSeconds(expirySeconds ?? 0);
        return await SetAsync(key, value, expiry, when).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null, When when = When.Always) where T : class
    {
        var fullKey = GetFullKey(key);
        var json = value.ToJson();

        var result = await _db.StringSetAsync(fullKey, json, expiry, when).ConfigureAwait(false);
        if (_enableLog)
        {
            _logger.LogDebug("Set<T> key: {Key}, expiry: {Expiry}, result: {Result}", fullKey, expiry, result);
        }
        return result;
    }

    #endregion

    #region 删除操作

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string key)
    {
        var fullKey = GetFullKey(key);
        var result = await _db.KeyDeleteAsync(fullKey).ConfigureAwait(false);
        if (_enableLog)
        {
            _logger.LogDebug("Delete key: {Key}, result: {Result}", fullKey, result);
        }
        return result;
    }

    /// <inheritdoc />
    public async Task<long> DeleteAsync(IEnumerable<string> keys)
    {
        var fullKeys = keys.Select(k => (RedisKey)GetFullKey(k)).ToArray();
        var result = await _db.KeyDeleteAsync(fullKeys).ConfigureAwait(false);
        if (_enableLog)
        {
            _logger.LogDebug("Delete keys: {Keys}, deleted: {Count}", string.Join(", ", fullKeys), result);
        }
        return result;
    }

    #endregion

    #region 存在性检查

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key)
    {
        var fullKey = GetFullKey(key);
        var result = await _db.KeyExistsAsync(fullKey).ConfigureAwait(false);
        if (_enableLog)
        {
            _logger.LogDebug("Exists key: {Key}, result: {Result}", fullKey, result);
        }
        return result;
    }

    #endregion

    #region 生存时间操作

    /// <inheritdoc />
    public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
    {
        var fullKey = GetFullKey(key);
        var result = await _db.KeyTimeToLiveAsync(fullKey).ConfigureAwait(false);
        if (_enableLog)
        {
            _logger.LogDebug("GetTTL key: {Key}, ttl: {TTL}", fullKey, result);
        }
        return result;
    }

    /// <inheritdoc />
    public async Task<bool> SetExpireAsync(string key, TimeSpan expiry)
    {
        var fullKey = GetFullKey(key);
        var result = await _db.KeyExpireAsync(fullKey, expiry).ConfigureAwait(false);
        if (_enableLog)
        {
            _logger.LogDebug("SetExpire key: {Key}, expiry: {Expiry}, result: {Result}", fullKey, expiry, result);
        }
        return result;
    }

    /// <inheritdoc />
    public async Task<bool> SetExpireAtAsync(string key, DateTime expireAt)
    {
        var fullKey = GetFullKey(key);
        var result = await _db.KeyExpireAsync(fullKey, expireAt).ConfigureAwait(false);
        if (_enableLog)
        {
            _logger.LogDebug("SetExpireAt key: {Key}, expireAt: {ExpireAt}, result: {Result}", fullKey, expireAt, result);
        }
        return result;
    }

    /// <inheritdoc />
    public async Task<bool> RefreshExpireAsync(string key, TimeSpan expiry)
    {
        var fullKey = GetFullKey(key);

        if (!await _db.KeyExistsAsync(fullKey).ConfigureAwait(false))
        {
            if (_enableLog)
            {
                _logger.LogDebug("RefreshExpire key: {Key} does not exist", fullKey);
            }
            return false;
        }

        var result = await _db.KeyExpireAsync(fullKey, expiry).ConfigureAwait(false);
        if (_enableLog)
        {
            _logger.LogDebug("RefreshExpire key: {Key}, expiry: {Expiry}, result: {Result}", fullKey, expiry, result);
        }
        return result;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveExpireAsync(string key)
    {
        var fullKey = GetFullKey(key);
        var result = await _db.KeyPersistAsync(fullKey).ConfigureAwait(false);
        if (_enableLog)
        {
            _logger.LogDebug("RemoveExpire key: {Key}, result: {Result}", fullKey, result);
        }
        return result;
    }

    #endregion
}