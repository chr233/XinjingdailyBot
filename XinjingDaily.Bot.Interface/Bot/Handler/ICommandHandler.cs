using System.Reflection;
using XinjingDaily.Bot.Infrastructure.Attribute;
using XinjingDaily.Bot.Infrastructure.Enums;

namespace XinjingDaily.Bot.Interface.Bot.Handler;

public interface ICommandHandler
{
    void RegisterQueryCommand(Type classType, MethodInfo methodInfo, PermissionAttribute permission, QueryCommandAttribute attribute);
    void RegisterQueryCommand(Type classType, MethodInfo methodInfo, ECommandScope scope, string? permission, QueryCommandAttribute attribute);
    void RegisterQueryCommand(Type classType, MethodInfo methodInfo, QueryCommandAttribute attribute);
    void RegisterTextCommand(Type classType, MethodInfo methodInfo, PermissionAttribute permission, TextCommandAttribute attribute);
    void RegisterTextCommand(Type classType, MethodInfo methodInfo, ECommandScope scope, string? permission, TextCommandAttribute attribute);
    void RegisterTextCommand(Type classType, MethodInfo methodInfo, TextCommandAttribute attribute);
}