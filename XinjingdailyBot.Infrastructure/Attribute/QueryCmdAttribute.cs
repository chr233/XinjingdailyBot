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
        /// 指令描述
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// 需求的权限
        /// </summary>
        public UserRights Rights { get; set; } = UserRights.None;

        /// <summary>
        /// 创建特性
        /// </summary>
        /// <param name="command"></param>
        public QueryCmdAttribute(string command)
        {
            Command = command;
            Rights = UserRights.None;
        }
        /// <summary>
        /// 创建特性
        /// </summary>
        /// <param name="command"></param>
        /// <param name="rights"></param>
        public QueryCmdAttribute(string command, UserRights rights)
        {
            Command = command;
            Rights = rights;
        }
    }
}
