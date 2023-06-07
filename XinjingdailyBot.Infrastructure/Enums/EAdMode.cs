namespace XinjingdailyBot.Infrastructure.Enums;

/// <summary>
/// 广告发布位置
/// </summary>
[Flags]
public enum EAdMode
{
    /// <summary>
    /// 不发布
    /// </summary>
    None = 0,
    /// <summary>
    /// 发布频道
    /// </summary>
    AcceptChannel = 1 << 0,
    /// <summary>
    /// 拒绝频道
    /// </summary>
    RejectChannel = 1 << 1,
    /// <summary>
    /// 审核频道
    /// </summary>
    ReviewGroup = 1 << 2,
    /// <summary>
    /// 评论区
    /// </summary>
    CommentGroup = 1 << 3,
    /// <summary>
    /// 闲聊群
    /// </summary>
    SubGroup = 1 << 4,

    /// <summary>
    /// 所有公开位置
    /// </summary>
    AllPublic = AcceptChannel | RejectChannel | CommentGroup | SubGroup,

    /// <summary>
    /// 所有位置
    /// </summary>
    All = AcceptChannel | RejectChannel | ReviewGroup | CommentGroup | SubGroup
}
