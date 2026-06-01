using XinjingDaily.Bot.Infrastructure.Bot.Context;

namespace XinjingDaily.Bot.IRepository.Context;

/// <summary>
/// Context 专用 Redis 仓储。
/// 所有方法在 Redis 不可用时返回 null / false，不抛异常。
/// </summary>
public interface IContextRedisRepository
{
    /// <summary>读取用户上下文（失败返回 null）</summary>
    Task<ContextRedisDto?> GetUserContextAsync(int userId, long chatId);

    /// <summary>写入用户上下文（失败返回 false）</summary>
    Task<bool> SetUserContextAsync(int userId, long chatId, ContextRedisDto dto);

    /// <summary>读取群聊公共上下文（失败返回 null）</summary>
    Task<ContextRedisDto?> GetChatContextAsync(string command, long chatId);

    /// <summary>写入群聊公共上下文（失败返回 false）</summary>
    Task<bool> SetChatContextAsync(string command, long chatId, ContextRedisDto dto);
}
