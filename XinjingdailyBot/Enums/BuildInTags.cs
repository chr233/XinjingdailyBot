namespace XinjingdailyBot.Enums
{
    [Flags]
    internal enum BuildInTags : byte
    {
        /// <summary>
        /// NSFW
        /// </summary>
        NSFW = 0x01,
        /// <summary>
        /// 我有一个朋友
        /// </summary>
        Friend = 0x02,
    }
}
