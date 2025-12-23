using XinjingdailyBot.Infrastructure.Localization;

namespace XinjingdailyBot.Infrastructure.Configs;

/// <summary>
/// 稿件选项
/// </summary>
public sealed record PostOption
{
    /// <summary>
    /// 启用每日投稿限制
    /// </summary>
    public bool EnablePostLimit { get; set; }
    /// <summary>
    /// 待定稿件上限, 不受 Ratio 倍率影响
    /// </summary>
    public int DailyPaddingLimit { get; set; } = 5;
    /// <summary>
    /// 审核队列上限
    /// </summary>
    public int DailyReviewLimit { get; set; } = 5;
    /// <summary>
    /// 每日投稿上限
    /// </summary>
    public int DailyPostLimit { get; set; } = 5;
    /// <summary>
    /// Ratio = 通过稿件数量 / RatioDivisor + 1
    /// 实际上限 = Ratio * 原始上限
    /// </summary>
    public int RatioDivisor { get; set; } = 100;
    /// <summary>
    /// 最高倍数
    /// </summary>
    public int MaxRatio { get; set; } = 10;
    /// <summary>
    /// 过滤连续空格
    /// </summary>
    public bool PureReturns { get; set; } = true;
    /// <summary>
    /// 过滤其他 #Tag
    /// </summary>
    public bool PureHashTag { get; set; } = true;
    /// <summary>
    /// 过滤字符串
    /// </summary>
    public string PureWords { get; set; } = Emojis.PureStrings;
    /// <summary>
    /// 稿件自动过期时间
    /// </summary>
    public uint PostExpiredTime { get; set; } = 3;

    public int MediaGroupReceiveTtl { get; set; } = 30;
}
