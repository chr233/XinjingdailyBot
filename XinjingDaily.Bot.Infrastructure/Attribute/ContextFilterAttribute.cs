namespace XinjingDaily.Bot.Infrastructure.Attribute;


/// <summary>
/// 用于标记Query命令
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class ContextFilterAttribute : System.Attribute
{
    /// <summary>
    /// 上下文名称
    /// </summary>
    public string? Mode { get; init; }
    /// <summary>
    /// 上下文别名
    /// </summary>
    public string? Alias { get; init; }

    /// <summary>
    /// 上下文名称
    /// </summary>
    /// <param name="context"></param>
    public ContextFilterAttribute(string mode)
    {
        Mode = mode;
    }
}
