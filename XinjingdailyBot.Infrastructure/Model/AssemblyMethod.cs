using System.Reflection;
using XinjingdailyBot.Infrastructure.Enums;

namespace XinjingdailyBot.Infrastructure.Model
{
    public sealed record AssemblyMethod
    {
        public MethodInfo Method { get; set; }
        public string? Description { get; set; }
        public EUserRights Rights { get; set; }

        public AssemblyMethod(MethodInfo method, string? description, EUserRights rights)
        {
            Method = method;
            Description = description;
            Rights = rights;
        }
    }
}
