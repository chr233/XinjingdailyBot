namespace XinjingdailyBot.Infrastructure.Configs;

/// <summary>
/// 等级经验值设定
/// </summary>
public sealed record LevelOption
{
    /// <summary>
    /// 每个通过稿件获得的经验
    /// </summary>
    public float ExpPerAccept { get; set; }
    /// <summary>
    /// 每个拒绝稿件获得的经验
    /// </summary>
    public float ExpPerReject { get; set; }
    /// <summary>
    /// 每个审核稿件获得的经验
    /// </summary>
    public float ExpPerReview { get; set; }
    /// <summary>
    /// 每个过期稿件获得的经验
    /// </summary>
    public float ExpPerExpire { get; set; }
}
