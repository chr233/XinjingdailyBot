using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.IRepository.Redis;

namespace XinjingDaily.Bot.Repository.Redis;

[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public sealed class RedisCacheRepository(
    IConnectionMultiplexer multiplexer,
    IOptions<AppSettings> options,
    ILogger<RedisCacheRepository> logger) : IRedisCacheRepository
{
    private readonly ILogger<RedisCacheRepository> _logger = logger;
    private readonly IDatabase _db = multiplexer.GetDatabase(options.Value.Redis.DefaultDatabase);
    private readonly string _keyPrefix = options.Value.Redis.KeyPrefix ?? string.Empty;


    public async Task SetAsync(string key, string value, TimeSpan? expiry = null, When when = When.Always)
    {
        await _db.StringSetAsync(key, value, expiry, When.Always);
        _logger.LogInformation("[Redis] Set key: {Key} with expiry: {Expiry}", key, expiry);
    }

    public async Task<string?> GetAsync(string key)
    {
        var value = await _db.StringGetAsync(key);
        _logger.LogInformation("[Redis] Get key: {Key} with value: {Value}", key, value);
        return value.HasValue ? value.ToString() : null;
    }
}
