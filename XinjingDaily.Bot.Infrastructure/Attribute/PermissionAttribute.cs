using XinjingDaily.Bot.Infrastructure.Enums;
using XinjingDaily.Bot.Infrastructure.Strings;

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
    public ECommandScope Scope { get; init; }

    /// <summary>
    /// 需要的权限
    /// </summary>
    public string? Permission { get; init; }

    /// <summary>
    /// 命令权限设定
    /// </summary>
    /// <param name="scope"></param>
    public PermissionAttribute(ECommandScope scope)
    {
        Scope = scope;
    }

    /// <summary>
    /// 命令权限设定
    /// </summary>
    /// <param name="permission"></param>
    public PermissionAttribute(string? permission) : this(ECommandScope.All)
    {
        Permission = permission;
    }

    /// <summary>
    /// 命令权限设定
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="permission"></param>
    public PermissionAttribute(ECommandScope scope, string? permission) : this(scope)
    {
        Permission = permission;
    }

    /// <summary>
    /// 命令权限设定
    /// </summary>
    /// <param name="permission"></param>
    public PermissionAttribute(EPermission permission) : this(ECommandScope.All)
    {
        Permission = Permissions.ToPermissionString(permission);
    }

    /// <summary>
    /// 命令权限设定
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="permission"></param>
    public PermissionAttribute(ECommandScope scope, EPermission permission) : this(scope)
    {
        Permission = Permissions.ToPermissionString(permission);
    }
}
