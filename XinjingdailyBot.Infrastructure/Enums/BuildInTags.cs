namespace XinjingdailyBot.Infrastructure.Enums
{
    /// <summary>
    /// 投稿Tag
    /// </summary>
    [Flags]
    public enum BuildInTags : byte
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// NSFW
        /// </summary>
        NSFW = 1,
        /// <summary>
        /// 我有一个朋友
        /// </summary>
        Friend = 2,
        /// <summary>
        /// 晚安
        /// </summary>
        WanAn = 4,
        /// <summary>
        /// AI怪图
        /// </summary>
        AIGraph = 8,
        /// <summary>
        /// 遮罩
        /// </summary>
        Spoiler = 16
    }
}
