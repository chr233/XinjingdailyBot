namespace XinjingdailyBot.Enums
{
    internal enum PostStatus : int
    {
        /// <summary>
        /// 默认状态
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 未投稿,等待确认
        /// </summary>
        Padding,
        /// <summary>
        /// 已取消
        /// </summary>
        Cancel,
        /// <summary>
        /// 已投稿,待审核
        /// </summary>
        Reviewing,
        /// <summary>
        /// 投稿未过审
        /// </summary>
        Rejected,
        /// <summary>
        /// 已过审并发布
        /// </summary>
        Accepted,
    }
}
