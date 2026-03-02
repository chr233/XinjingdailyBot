using System.Reflection;
using XinjingDaily.Bot.Infrastructure.Attribute;

namespace XinjingDaily.Bot.Interface.Bot.Handler;

public interface ICommandHandler
{
    void RegisterQueryCommand(Type classType, MethodInfo methodInfo, QueryCmdAttribute attribute);
    void RegisterTextCommand(Type classType, MethodInfo methodInfo, TextCmdAttribute attribute);
}