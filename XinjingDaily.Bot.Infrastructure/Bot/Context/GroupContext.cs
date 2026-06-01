namespace XinjingDaily.Bot.Infrastructure.Bot.Context;

/// <summary>
/// 群/频道上下文，包含两个懒加载层级：
///   - 用户在群内（User）：每个用户独立，默认操作；
///   - 群组公共（Chat）：群内所有人共享，Chat 前缀方法操作。
/// 两个 Store 各自独立触发 IO，互不干扰。
/// </summary>
public sealed class GroupContext
{
    internal LazyContextStore UserStore { get; }
    internal LazyContextStore ChatStore { get; }

    internal GroupContext(LazyContextStore userStore, LazyContextStore chatStore)
    {
        UserStore = userStore;
        ChatStore = chatStore;
    }

    // ── 用户在群内（默认操作）──────────────────────────

    /// <summary>同步获取用户 Mode（仅已加载时有值）</summary>
    public string Mode => UserStore.IsLoaded ? UserStore.GetMode() : string.Empty;

    public ValueTask<string> GetModeAsync() => UserStore.GetModeAsync();
    public ValueTask SetModeAsync(string mode) => UserStore.SetModeAsync(mode);
    public ValueTask ClearModeAsync() => UserStore.ClearModeAsync();

    public ValueTask<T?> GetAsync<T>(string key) => UserStore.GetAsync<T>(key);
    public ValueTask SetAsync<T>(string key, T value) where T : notnull => UserStore.SetAsync(key, value);
    public ValueTask RemoveAsync(string key) => UserStore.RemoveAsync(key);
    public ValueTask ClearAsync() => UserStore.ClearAsync();

    // ── 群组公共（Chat 前缀）────────────────────────────

    /// <summary>同步获取群组 Mode（仅已加载时有值）</summary>
    public string ChatMode => ChatStore.IsLoaded ? ChatStore.GetMode() : string.Empty;

    public ValueTask<string> GetChatModeAsync() => ChatStore.GetModeAsync();
    public ValueTask SetChatModeAsync(string mode) => ChatStore.SetModeAsync(mode);
    public ValueTask ClearChatModeAsync() => ChatStore.ClearModeAsync();

    public ValueTask<T?> ChatGetAsync<T>(string key) => ChatStore.GetAsync<T>(key);
    public ValueTask ChatSetAsync<T>(string key, T value) where T : notnull => ChatStore.SetAsync(key, value);
    public ValueTask ChatRemoveAsync(string key) => ChatStore.RemoveAsync(key);
    public ValueTask ClearChatAsync() => ChatStore.ClearAsync();

    // ── 内部 ──────────────────────────────────────────
    internal bool IsUserDirty => UserStore.IsDirty;
    internal bool IsChatDirty => ChatStore.IsDirty;
}
