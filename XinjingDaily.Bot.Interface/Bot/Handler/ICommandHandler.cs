using System.Reflection;
using XinjingDaily.Bot.Infrastructure.Attribute;

namespace XinjingDaily.Bot.Interface.Bot.Handler;

public interface ICommandHandler
{
    void RegisterQueryCommand(Type classType, MethodInfo methodInfo, QueryCommandAttribute attribute);
    void RegisterTextCommand(Type classType, MethodInfo methodInfo, TextCommandAttribute attribute);
}