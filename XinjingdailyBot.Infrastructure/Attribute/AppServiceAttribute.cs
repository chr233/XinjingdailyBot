namespace XinjingdailyBot.Infrastructure.Attribute
{
    /// <summary>
    /// 标记服务
    /// 1、如果服务是本身 直接在类上使用[AppService]
    /// 2、如果服务是接口 在类上使用 [AppService(ServiceType = typeof(实现接口))]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class AppServiceAttribute : System.Attribute
    {
        /// <summary>
        /// 服务声明周期
        /// 不给默认值的话注册的是AddSingleton
        /// </summary>
        public LifeTime ServiceLifetime { get; set; } = LifeTime.Scoped;
        /// <summary>
        /// 指定服务类型
        /// </summary>
        public Type? ServiceType { get; set; }
        /// <summary>
        /// 是否可以从第一个接口获取服务类型
        /// </summary>
        public bool InterfaceServiceType { get; set; }

        [Obsolete]
        public AppServiceAttribute()
        {
        }

        public AppServiceAttribute(LifeTime serviceLifetime)
        {
            ServiceLifetime = serviceLifetime;
        }

        public AppServiceAttribute(Type? serviceType, LifeTime serviceLifetime)
        {
            ServiceLifetime = serviceLifetime;
            ServiceType = serviceType;
        }

        public AppServiceAttribute(Type? serviceType, LifeTime serviceLifetime, bool interfaceServiceType)
        {
            ServiceLifetime = serviceLifetime;
            ServiceType = serviceType;
            InterfaceServiceType = interfaceServiceType;
        }
    }

    public enum LifeTime
    {
        /// <summary>
        /// 瞬时
        /// </summary>
        Transient,
        /// <summary>
        /// 范围
        /// </summary>
        Scoped,
        /// <summary>
        /// 单例
        /// </summary>
        Singleton
    }
}
