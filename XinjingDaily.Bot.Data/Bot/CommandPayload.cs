using System.Reflection;

namespace XinjingDaily.Bot.Data.Bot;

public sealed record CommandPayload(Type ClassType, MethodInfo Method);

