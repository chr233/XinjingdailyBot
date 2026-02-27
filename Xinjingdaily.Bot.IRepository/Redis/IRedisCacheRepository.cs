using StackExchange.Redis;

namespace XinjingDaily.Bot.IRepository.Redis;

public interface IRedisCacheRepository
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value, TimeSpan? expiry = null, When when = When.Always);
}