using XinjingDaily.Bot.Infrastructure.Enums;

namespace XinjingDaily.Bot.Infrastructure.Attribute;


/// <summary>
/// 用于标记文本命令
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class PermissionAttribute : System.Attribute
{
    /// <summary>
    /// 命令可用范围
    /// </summary>
    public ECommandScope Scope { get; set; }

    /// <summary>
    /// 需要的权限
    /// </summary>
    public string? Permission { get; set; }

    /// <summary>
    /// 创建特性
    /// </summary>
    /// <param name="scope"></param>
    public PermissionAttribute(ECommandScope scope)
    {
        Scope = scope;
    }

    /// <summary>
    /// 创建特性
    /// </summary>
    /// <param name="permission"></param>
    public PermissionAttribute(string? permission) : this(ECommandScope.All)
    {
        Permission = permission;
    }

    /// <summary>
    /// 创建特性
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="permission"></param>
    public PermissionAttribute(ECommandScope scope, string? permission) : this(scope)
    {
        Permission = permission;
    }
}
