using System.Reflection;
using XinjingdailyBot.Infrastructure.Enums;

namespace XinjingdailyBot.Infrastructure.Model;

/// <summary>
/// 反射方法
/// </summary>
public sealed record AssemblyMethod
{
    /// <summary>
    /// 反射方法
    /// </summary>
    public MethodInfo Method { get; set; }
    /// <summary>
    /// 命令说明
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// 命令权限
    /// </summary>
    public EUserRights Rights { get; set; }

    /// <summary>
    /// 反射方法
    /// </summary>
    /// <param name="method"></param>
    /// <param name="description"></param>
    /// <param name="rights"></param>
    public AssemblyMethod(MethodInfo method, string? description, EUserRights rights)
    {
        Method = method;
        Description = description;
        Rights = rights;
    }
}
