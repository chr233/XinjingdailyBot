namespace XinjingDaily.Bot.Infrastructure.Attribute;

/// <summary>
/// 用于标记文本命令
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class MessageHandlerAttribute : System.Attribute
{
    /// <summary>
    /// 指令名称
    /// </summary>
    public string? Context { get; init; }

    public bool IsShareContext { get; init; }

    /// <summary>
    /// 文字命令
    /// </summary>
    /// <param name="context"></param>
    public MessageHandlerAttribute(string? context)
    {
        Context = context;
    }
}