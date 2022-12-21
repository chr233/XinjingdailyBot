namespace XinjingdailyBot.Infrastructure.Attribute
{
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
