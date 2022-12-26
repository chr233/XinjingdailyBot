using XinjingdailyBot.Infrastructure.Enums;

namespace XinjingdailyBot.Infrastructure.Attribute
{

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class QueryCmdAttribute : System.Attribute
    {
        /// <summary>
        /// 指定服务类型
        /// </summary>
        public string Command { get; set; } = "";

        public bool ValidUser { get; set; } = true;

        /// <summary>
        /// 需求的权限
        /// </summary>
        public UserRights RequireRights { get; set; } = UserRights.None;
    }
}
