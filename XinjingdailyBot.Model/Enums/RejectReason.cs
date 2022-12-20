namespace XinjingdailyBot.Model.Enums
{
    /// <summary>
    /// 拒稿原因
    /// </summary>
    public enum RejectReason : byte
    {
        /// <summary>
        /// 未拒绝
        /// </summary>
        NotReject = 0,
        /// <summary>
        /// 图片模糊
        /// </summary>
        Fuzzy,
        /// <summary>
        /// 重复稿件
        /// </summary>
        Duplicate,
        /// <summary>
        /// 无趣
        /// </summary>
        Boring,
        /// <summary>
        /// 内容不接受
        /// </summary>
        Deny,
        /// <summary>
        /// 其他原因
        /// </summary>
        Other,
        /// <summary>
        /// 牛皮癣二维码
        /// </summary>
        QRCode,
        /// <summary>
        /// 自定义原因
        /// </summary>
        CustomReason,
        /// <summary>
        /// 令人迷惑
        /// </summary>
        Confused,
        /// <summary>
        /// 审核超时,自动拒稿
        /// </summary>
        AutoReject = 255,

    }
}
