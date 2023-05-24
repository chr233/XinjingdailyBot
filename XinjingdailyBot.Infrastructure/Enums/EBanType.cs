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
        GlobalMute,
        GlobalBan,
        GlobalUnMute,
        GlobalUnBan,
    }
}
