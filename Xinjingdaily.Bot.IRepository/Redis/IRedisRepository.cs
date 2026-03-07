using StackExchange.Redis;

namespace XinjingDaily.Bot.IRepository.Redis;

public interface IRedisRepository
{
    #region String 类型操作

    /// <summary>
    /// 获取字符串值
    /// </summary>
    Task<string?> GetStringAsync(string key);

    /// <summary>
    /// 设置字符串值
    /// </summary>
    Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null, When when = When.Always);

    #endregion

    #region 泛型操作

    /// <summary>
    /// 获取对象（JSON 反序列化）
    /// </summary>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// 设置对象（JSON 序列化）
    /// </summary>
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null, When when = When.Always) where T : class;

    #endregion

    #region 删除操作

    /// <summary>
    /// 删除键
    /// </summary>
    Task<bool> DeleteAsync(string key);

    /// <summary>
    /// 批量删除键
    /// </summary>
    Task<long> DeleteAsync(IEnumerable<string> keys);

    #endregion

    #region 存在性检查

    /// <summary>
    /// 检查键是否存在
    /// </summary>
    Task<bool> ExistsAsync(string key);

    #endregion

    #region 生存时间操作

    /// <summary>
    /// 获取键的剩余生存时间
    /// </summary>
    Task<TimeSpan?> GetTimeToLiveAsync(string key);

    /// <summary>
    /// 设置键的生存时间
    /// </summary>
    Task<bool> SetExpireAsync(string key, TimeSpan expiry);

    /// <summary>
    /// 设置键的过期时间点
    /// </summary>
    Task<bool> SetExpireAtAsync(string key, DateTime expireAt);

    /// <summary>
    /// 刷新键的生存时间（重新设置过期时间）
    /// </summary>
    Task<bool> RefreshExpireAsync(string key, TimeSpan expiry);

    /// <summary>
    /// 移除键的生存时间（使键永不过期）
    /// </summary>
    Task<bool> RemoveExpireAsync(string key);
    Task<bool> SetAsync<T>(string key, T value, int? expirySecond = null, When when = When.Always) where T : class;
    Task<bool> SetStringAsync(string key, string value, int? expirySeconds = null, When when = When.Always);

    #endregion
}