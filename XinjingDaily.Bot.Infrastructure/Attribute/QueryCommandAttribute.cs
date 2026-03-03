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

    public bool IsShareContext { get; init; }

    /// <summary>
    /// 创建特性
    /// </summary>
    /// <param name="command"></param>
    public QueryCommandAttribute(string command)
    {
        Command = command;
    }

    public QueryCommandAttribute(string command, bool isShareContext) : this(command)
    {
        IsShareContext = isShareContext;
    }

    public QueryCommandAttribute(string command, string? alias) : this(command)
    {
        Alias = alias;
    }

    public QueryCommandAttribute(string command, string? alias, bool isShareContext) : this(command)
    {
        Alias = alias;
        IsShareContext = isShareContext;
    }
}
