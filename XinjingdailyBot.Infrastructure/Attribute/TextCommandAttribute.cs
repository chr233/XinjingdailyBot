using XinjingdailyBot.Infrastructure.Enums;

namespace XinjingdailyBot.Infrastructure.Attribute
{
    /// <summary>
    /// 参考地址：https://www.cnblogs.com/kelelipeng/p/10643556.html
    /// 标记服务
    /// 如何使用？
    /// 1、如果服务是本身 直接在类上使用[AppService]
    /// 2、如果服务是接口 在类上使用 [AppService(ServiceType = typeof(实现接口))]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class TextCommandAttribute : System.Attribute
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
}
