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

    /// <summary>
    /// 是否共享上下文, 在群组中不同的人使用同一个上下文, 不影响私聊
    /// </summary>
    public bool IsShareContext { get; init; }

    /// <summary>
    /// 文字命令
    /// </summary>
    /// <param name="command"></param>
    public TextCommandAttribute(string command)
    {
        Command = command;
    }

    /// <summary>
    /// 文字命令
    /// </summary>
    /// <param name="command"></param>
    /// <param name="isShareContext"></param>
    public TextCommandAttribute(string command, bool isShareContext) : this(command)
    {
        IsShareContext = isShareContext;
    }

    /// <summary>
    /// 文字命令
    /// </summary>
    /// <param name="command"></param>
    /// <param name="description"></param>
    public TextCommandAttribute(string command, string? description) : this(command)
    {
        Description = description;
    }

    /// <summary>
    /// 文字命令
    /// </summary>
    /// <param name="command"></param>
    /// <param name="description"></param>
    /// <param name="isShareContext"></param>
    public TextCommandAttribute(string command, string? description, bool isShareContext) : this(command)
    {
        Description = description;
        IsShareContext = isShareContext;
    }

    /// <summary>
    /// 文字命令
    /// </summary>
    /// <param name="command"></param>
    /// <param name="alias"></param>
    /// <param name="description"></param>
    /// <param name="isShareContext"></param>
    public TextCommandAttribute(string command, string? alias, string? description) : this(command)
    {
        Alias = alias;
        Description = description;
    }

    /// <summary>
    /// 文字命令
    /// </summary>
    /// <param name="command"></param>
    /// <param name="alias"></param>
    /// <param name="description"></param>
    /// <param name="isShareContext"></param>
    public TextCommandAttribute(string command, string? alias, string? description, bool isShareContext) : this(command)
    {
        Alias = alias;
        Description = description;
        IsShareContext = isShareContext;
    }
}