namespace XinjingdailyBot.Infrastructure.Enums
{
    /// <summary>
    /// 频道封禁类型
    /// </summary>
    public enum ChannelOption : byte
    {
        /// <summary>
        /// 不做处理
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 去除来源, 其他不做处理
        /// </summary>
        PurgeOrigin = 1,
        /// <summary>
        /// 自动拒绝, 不进入审核流程
        /// </summary>
        AutoReject = 2,
    }
}
