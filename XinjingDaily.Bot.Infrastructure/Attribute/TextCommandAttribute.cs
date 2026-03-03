namespace XinjingDaily.Bot.Infrastructure.Attribute;


/// <summary>
/// 用于标记文本命令
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class TextCommandAttribute : System.Attribute
{
    /// <summary>
    /// 指令名称
    /// </summary>
    public string Command { get; init; }
    /// <summary>
    /// 指令别名, 用 | 分隔
    /// </summary>
    public string? Alias { get; init; }
    /// <summary>
    /// 指令描述
    /// </summary>
    public string? Description { get; init; }

    public bool IsShareContext { get; init; }

    /// <summary>
    /// 创建特性
    /// </summary>
    /// <param name="command"></param>
    public TextCommandAttribute(string command)
    {
        Command = command;
    }

    public TextCommandAttribute(string command, bool isShareContext) : this(command)
    {
        IsShareContext = isShareContext;
    }

    public TextCommandAttribute(string command, string? description) : this(command)
    {
        Description = description;
    }

    public TextCommandAttribute(string command, string? description, bool isShareContext) : this(command)
    {
        Description = description;
        IsShareContext = isShareContext;
    }

    public TextCommandAttribute(string command, string? alias, string? description, bool isShareContext) : this(command)
    {
        Alias = alias;
        Description = description;
        IsShareContext = isShareContext;
    }
}