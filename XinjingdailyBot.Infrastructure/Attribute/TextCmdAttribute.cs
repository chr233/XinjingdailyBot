using XinjingdailyBot.Infrastructure.Enums;

namespace XinjingdailyBot.Infrastructure.Attribute
{

    /// <summary>
    /// 用于标记文本命令
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class TextCmdAttribute : System.Attribute
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
        public EUserRights Rights { get; set; } = EUserRights.None;

        /// <summary>
        /// 创建特性
        /// </summary>
        /// <param name="command"></param>
        public TextCmdAttribute(string command)
        {
            Command = command;
            Rights = EUserRights.None;
        }
        /// <summary>
        /// 创建特性
        /// </summary>
        /// <param name="command"></param>
        /// <param name="rights"></param>
        public TextCmdAttribute(string command, EUserRights rights)
        {
            Command = command;
            Rights = rights;
        }
    }
}
