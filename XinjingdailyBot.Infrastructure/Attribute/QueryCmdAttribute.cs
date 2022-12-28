using XinjingdailyBot.Infrastructure.Enums;

namespace XinjingdailyBot.Infrastructure.Attribute
{

    /// <summary>
    /// 用于标记Query命令
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class QueryCmdAttribute : System.Attribute
    {
        /// <summary>
        /// 指令名称
        /// </summary>
        public string Command { get; set; } = "";
        /// <summary>
        /// 指令别名
        /// </summary>
        public string? Alias { get; set; }
        /// <summary>
        /// 需求的权限
        /// </summary>
        public UserRights Rights { get; set; } = UserRights.None;
        /// <summary>
        /// 是否验证UserId
        /// </summary>
        public bool ValidUser { get; set; }

        /// <summary>
        /// 创建特性
        /// </summary>
        /// <param name="command"></param>
        /// <param name="rights"></param>
        public QueryCmdAttribute(string command, UserRights rights = UserRights.None)
        {
            Command = command;
            Rights = rights;
        }
    }
}
