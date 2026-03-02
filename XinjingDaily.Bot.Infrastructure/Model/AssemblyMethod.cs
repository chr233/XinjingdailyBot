using System.Reflection;
using XinjingDaily.Bot.Infrastructure.Enums;

namespace XinjingDaily.Bot.Infrastructure.Model;

/// <summary>
/// 程序集方法信息
/// </summary>
/// <param name="Method">方法信息</param>
/// <param name="Description">方法描述</param>
/// <param name="Rights">需要的权限</param>
/// <param name="Scope">适用的聊天场景</param>
public sealed record AssemblyMethod(
    MethodInfo Method,
    string? Description,
    EUserRights Rights,
    ECommandScope Scope = ECommandScope.All);

