namespace XinjingDaily.Bot.Infrastructure.Attribute;

/// <summary>
/// 标记 handler 需要的 Context Mode。
/// 当前上下文 Mode 与此值匹配时才调用该 handler；
/// 不标注此 Attribute 则为兜底 handler（无 Mode 过滤）。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class ContextFilterAttribute : System.Attribute
{
    /// <summary>需要匹配的 Context Mode 字符串</summary>
    public string Mode { get; }

    public ContextFilterAttribute(string mode) => Mode = mode;
}
