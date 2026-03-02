namespace XinjingDaily.Bot.Infrastructure.Enums;

/// <summary>
/// 发布频道类型
/// </summary>
public enum EPostChannelType : byte
{
    /// <summary>
    /// 发布频道/群组
    /// </summary>
    Accept = 0,
    /// <summary>
    /// 拒绝频道/群组
    /// </summary>
    Reject = 1,
    /// <summary>
    /// 操作频道/群组
    /// </summary>
    Log = 2,
}
