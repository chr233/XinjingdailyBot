using System.Reflection;
using XinjingdailyBot.Infrastructure.Enums;

namespace XinjingdailyBot.Infrastructure.Model
{
    public sealed record AssemblyMethod
    {
        public MethodInfo Method { get; set; }
        public string? Description { get; set; }
        public UserRights Rights { get; set; }

        public AssemblyMethod(MethodInfo method, string? description, UserRights rights)
        {
            Method = method;
            Description = description;
            Rights = rights;
        }
    }
}
