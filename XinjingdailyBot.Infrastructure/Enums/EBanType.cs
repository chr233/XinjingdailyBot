namespace XinjingdailyBot.Infrastructure.Enums
{
    /// <summary>
    /// 封禁类型
    /// </summary>
    public enum EBanType : byte
    {
        /// <summary>
        /// 解封
        /// </summary>
        UnBan = 0,
        /// <summary>
        /// 封禁
        /// </summary>
        Ban,
        /// <summary>
        /// 警告
        /// </summary>
        Warning,
        /// <summary>
        /// 全局禁言
        /// </summary>
        GlobalMute,
        /// <summary>
        /// 全局封禁
        /// </summary>
        GlobalBan,
        /// <summary>
        /// 全局解除禁言
        /// </summary>
        GlobalUnMute,
        /// <summary>
        /// 全局解除封禁
        /// </summary>
        GlobalUnBan,
    }
}
