using XinjingdailyBot.Infrastructure.Enums;

namespace XinjingdailyBot.Infrastructure.Attribute
{

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class CommandAttribute : System.Attribute
    {
        /// <summary>
        /// 需求的权限
        /// </summary>
        public UserRights RequireRights { get; set; } = UserRights.None;
        /// <summary>
        /// 指定服务类型
        /// </summary>
        public string Command { get; set; } = "";
    }


    /// <summary>
    /// 命令类型
    /// </summary>
    public enum CmdType
    {
        Text,
        Query,
    }

    
}
