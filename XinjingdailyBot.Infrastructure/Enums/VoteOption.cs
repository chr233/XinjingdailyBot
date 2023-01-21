namespace XinjingdailyBot.Infrastructure.Enums
{
    /// <summary>
    /// 审核投票
    /// </summary>
    public enum VoteOption : byte
    {
        /// <summary>
        /// 未表态
        /// </summary>
        None = 0,

        /// <summary>
        /// 积极
        /// </summary>
        Positive,

        /// <summary>
        /// 消极
        /// </summary>
        Negative,
    }
}
