namespace XinjingDaily.Bot.Infrastructure.Attribute;


/// <summary>
/// 用于标记Query命令
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class QueryCommandAttribute : System.Attribute
{
    /// <summary>
    /// 指令名称
    /// </summary>
    public string Command { get; init; }
    /// <summary>
    /// 指令别名
    /// </summary>
    public string? Alias { get; init; }

    /// <summary>
    /// 是否共享上下文, 在群组中不同的人使用同一个上下文, 不影响私聊
    /// </summary>
    public bool IsShareContext { get; init; }

    /// <summary>
    /// Query命令
    /// </summary>
    /// <param name="command"></param>
    public QueryCommandAttribute(string command)
    {
        Command = command;
    }

    /// <summary>
    /// Query命令
    /// </summary>
    /// <param name="command"></param>
    /// <param name="isShareContext"></param>
    public QueryCommandAttribute(string command, bool isShareContext) : this(command)
    {
        IsShareContext = isShareContext;
    }

    /// <summary>
    /// Query命令
    /// </summary>
    /// <param name="command"></param>
    /// <param name="alias"></param>
    public QueryCommandAttribute(string command, string? alias) : this(command)
    {
        Alias = alias;
    }

    /// <summary>
    /// Query命令
    /// </summary>
    /// <param name="command"></param>
    /// <param name="alias"></param>
    /// <param name="isShareContext"></param>
    public QueryCommandAttribute(string command, string? alias, bool isShareContext) : this(command)
    {
        Alias = alias;
        IsShareContext = isShareContext;
    }
}
