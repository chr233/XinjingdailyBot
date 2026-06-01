using XinjingDaily.Bot.Infrastructure.Bot.Context;

namespace XinjingDaily.Bot.Interface.Bot.Context;

public interface IContextService
{
    /// <summary>
    /// 创建懒加载私聊上下文（无 IO，首次访问时触发加载）
    /// </summary>
    PrivateContext CreatePrivateContext(int userId, long chatId);

    /// <summary>
    /// 创建懒加载群聊上下文（无 IO，首次访问时触发加载）
    /// </summary>
    GroupContext CreateGroupContext(int userId, long chatId, string command);

    /// <summary>
    /// 将私聊上下文脏数据写回 Redis + DB（IsDirty=false 时跳过）
    /// </summary>
    Task SavePrivateContextAsync(PrivateContext ctx);

    /// <summary>
    /// 将群聊上下文脏数据写回 Redis + DB（IsDirty=false 时跳过）
    /// </summary>
    Task SaveGroupContextAsync(GroupContext ctx);
}
