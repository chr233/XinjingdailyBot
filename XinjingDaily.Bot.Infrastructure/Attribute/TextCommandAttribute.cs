using XinjingDaily.Bot.Infrastructure.Enums;

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
    public string Command { get; set; }
    /// <summary>
    /// 指令别名, 用 | 分隔
    /// </summary>
    public string? Alias { get; set; }
    /// <summary>
    /// 指令描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 命令可用范围
    /// </summary>
    public ECommandScope Scope { get; set; }

    /// <summary>
    /// 需要的权限
    /// </summary>
    public string? Permission { get; set; }

    public bool IsShareContext { get; set; }

    /// <summary>
    /// 创建特性
    /// </summary>
    /// <param name="command"></param>
    public TextCommandAttribute(string command)
    {
        Command = command;
        Scope = ECommandScope.All;
    }
    /// <summary>
    /// 创建特性
    /// </summary>
    /// <param name="command"></param>
    /// <param name="scope"></param>
    public TextCommandAttribute(string command, ECommandScope scope)
    {
        Command = command;
        Scope = scope;
    }
    /// <summary>
    /// 创建特性
    /// </summary>
    /// <param name="command"></param>
    /// <param name="scope"></param>
    /// <param name="permission"></param>
    public TextCommandAttribute(string command, ECommandScope scope, string? permission)
    {
        Command = command;
        Scope = scope;
        Permission = permission;
    }

    /// <summary>
    /// 创建特性
    /// </summary>
    /// <param name="command"></param>
    /// <param name="permission"></param>
    public TextCommandAttribute(string command, string? permission)
    {
        Command = command;
        Scope = ECommandScope.All;
        Permission = permission;
    }
}
