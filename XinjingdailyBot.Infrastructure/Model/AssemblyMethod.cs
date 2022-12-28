using System.Reflection;
using XinjingdailyBot.Infrastructure.Enums;

namespace XinjingdailyBot.Infrastructure.Model
{
    public sealed record AssemblyMethod
    {
        public MethodInfo Method { get; set; }
        public string? Description { get; set; }
        public UserRights Rights { get; set; }
        public bool ValidUser { get; set; }

        public AssemblyMethod(MethodInfo method, string? description, UserRights rights)
        {
            Method = method;
            Description = description;
            Rights = rights;
        }

        public AssemblyMethod(MethodInfo method, bool validUser, UserRights rights)
        {
            Method = method;
            ValidUser = validUser;
            Rights = rights;
        }
    }
}
