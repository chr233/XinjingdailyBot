using XinjingdailyBot.Infrastructure.Enums;

namespace XinjingdailyBot.Infrastructure.Attribute
{

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class TextCmdAttribute : System.Attribute
    {
        /// <summary>
        /// 指定服务类型
        /// </summary>
        public string Command { get; set; } = "";

        public string? Alias { get; set; }
        public string? Description { get; set; }
        /// <summary>
        /// 需求的权限
        /// </summary>
        public UserRights Rights { get; set; } = UserRights.None;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="alias"></param>
        /// <param name="description"></param>
        /// <param name="rights"></param>
        public TextCmdAttribute(string command, UserRights rights = UserRights.None)
        {
            Command = command;
            Rights = rights;
        }
    }
}
