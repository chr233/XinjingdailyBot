using System.Text.Json;

namespace XinjingDaily.Bot.Infrastructure.Bot.Context;

/// <summary>
/// 懒加载上下文存储。
/// IO（Redis/DB）推迟到第一次实际访问时触发，从未访问则零 IO。
/// </summary>
public sealed class LazyContextStore
{
    private const int MaxDataJsonLength = 2000;

    private readonly Func<Task<ContextRedisDto>> _loader;

    private ContextRedisDto? _dto;
    private Dictionary<string, string>? _data;
    private bool _loaded;
    private bool _isDirty;

    public LazyContextStore(Func<Task<ContextRedisDto>> loader)
    {
        _loader = loader;
    }

    /// <summary>是否已触发加载</summary>
    public bool IsLoaded => _loaded;

    /// <summary>是否有未保存的修改</summary>
    public bool IsDirty => _isDirty;

    // ── 异步确保已加载 ──────────────────────────────

    private async ValueTask EnsureLoadedAsync()
    {
        if (_loaded) return;
        _dto = await _loader().ConfigureAwait(false);
        _data = _dto.Data ?? [];
        _loaded = true;
    }

    // ── Mode ──────────────────────────────────────

    public async ValueTask<string> GetModeAsync()
    {
        await EnsureLoadedAsync().ConfigureAwait(false);
        return _dto!.Mode;
    }

    public async ValueTask SetModeAsync(string mode)
    {
        await EnsureLoadedAsync().ConfigureAwait(false);
        _dto!.Mode = mode;
        _isDirty = true;
    }

    public async ValueTask ClearModeAsync()
    {
        await EnsureLoadedAsync().ConfigureAwait(false);
        _dto!.Mode = string.Empty;
        _isDirty = true;
    }

    // ── KV ────────────────────────────────────────

    public async ValueTask<T?> GetAsync<T>(string key)
    {
        await EnsureLoadedAsync().ConfigureAwait(false);
        if (_data!.TryGetValue(key, out var json))
            try { return JsonSerializer.Deserialize<T>(json); } catch { }
        return default;
    }

    public async ValueTask SetAsync<T>(string key, T value) where T : notnull
    {
        await EnsureLoadedAsync().ConfigureAwait(false);
        var serialized = JsonSerializer.Serialize(value);
        _data!.TryGetValue(key, out var oldValue);
        _data[key] = serialized;

        var snapshot = JsonSerializer.Serialize(_data);
        if (snapshot.Length > MaxDataJsonLength)
        {
            if (oldValue is not null) _data[key] = oldValue;
            else _data.Remove(key);
            throw new InvalidOperationException(
                $"Context 数据超过上限 {MaxDataJsonLength} 字符（当前 {snapshot.Length} 字符），请精简存储内容。");
        }
        _isDirty = true;
    }

    public async ValueTask RemoveAsync(string key)
    {
        await EnsureLoadedAsync().ConfigureAwait(false);
        if (_data!.Remove(key)) _isDirty = true;
    }

    public async ValueTask ClearAsync()
    {
        await EnsureLoadedAsync().ConfigureAwait(false);
        _data!.Clear();
        _dto!.Mode = string.Empty;
        _isDirty = true;
    }

    // ── 内部：同步访问（仅在已加载后安全调用）──────────

    /// <summary>同步获取 Mode（仅已加载时有值，否则返回空字符串）</summary>
    internal string GetMode() => _dto?.Mode ?? string.Empty;

    /// <summary>获取 DB 主键</summary>
    internal int GetDbId() => _dto?.DbId ?? 0;

    /// <summary>导出 DTO 供保存。未加载或无脏数据时返回 null。</summary>
    internal ContextRedisDto? ExportDto()
    {
        if (!_loaded || !_isDirty) return null;
        return new ContextRedisDto
        {
            DbId = _dto!.DbId,
            UserId = _dto.UserId,
            ChatId = _dto.ChatId,
            Command = _dto.Command,
            Mode = _dto.Mode,
            Data = new Dictionary<string, string>(_data!),
            ModifyAt = DateTime.UtcNow
        };
    }

    internal string SerializeData() => JsonSerializer.Serialize(_data ?? []);

    internal void MarkClean() => _isDirty = false;
}
