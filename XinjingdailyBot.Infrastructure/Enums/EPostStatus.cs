namespace XinjingdailyBot.Infrastructure.Enums;

/// <summary>
/// 稿件状态
/// </summary>
public enum EPostStatus : int
{
    /// <summary>
    /// 确认投稿超时
    /// </summary>
    ConfirmTimeout = -2,
    /// <summary>
    /// 审核超时
    /// </summary>
    ReviewTimeout = -1,
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
    /// <summary>
    /// 已计划发布
    /// </summary>
    [Obsolete("废弃功能")]
    InPlan,
    /// <summary>
    /// 已过审并发布在第二频道
    /// </summary>
    AcceptSecond,
}
