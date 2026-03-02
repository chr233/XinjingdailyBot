using System.Reflection;

namespace XinjingDaily.Bot.Data.Bot;

public sealed record CommandDefinition<T>(Type ClassType, MethodInfo Method, T Attribute) where T : System.Attribute;

