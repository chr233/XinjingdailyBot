namespace XinjingDaily.Bot.Infrastructure.Bot.Context;

/// <summary>
/// 私聊上下文，每个用户在每个会话独立。
/// 所有 IO 推迟到首次实际访问时触发，从未访问则零 IO。
/// </summary>
public sealed class PrivateContext
{
    internal LazyContextStore Store { get; }

    internal PrivateContext(LazyContextStore store)
    {
        Store = store;
    }

    // ── Mode ──────────────────────────────────────

    /// <summary>同步获取 Mode（仅 Store 已加载时有值，否则返回空字符串）</summary>
    public string Mode => Store.IsLoaded ? Store.GetMode() : string.Empty;

    public ValueTask<string> GetModeAsync() => Store.GetModeAsync();
    public ValueTask SetModeAsync(string mode) => Store.SetModeAsync(mode);
    public ValueTask ClearModeAsync() => Store.ClearModeAsync();

    // ── KV ────────────────────────────────────────

    public ValueTask<T?> GetAsync<T>(string key) => Store.GetAsync<T>(key);
    public ValueTask SetAsync<T>(string key, T value) where T : notnull => Store.SetAsync(key, value);
    public ValueTask RemoveAsync(string key) => Store.RemoveAsync(key);
    public ValueTask ClearAsync() => Store.ClearAsync();

    // ── 内部 ──────────────────────────────────────
    internal bool IsDirty => Store.IsDirty;
}
