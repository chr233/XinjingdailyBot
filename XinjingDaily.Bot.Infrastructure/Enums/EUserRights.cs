namespace XinjingDaily.Bot.Infrastructure.Enums;

/// <summary>
/// 用户权限
/// </summary>
public enum EUserRights : byte
{
    /// <summary>
    /// 无权限
    /// </summary>
    None = 0,

    /// <summary>
    /// 投稿
    /// </summary>
    SendPost = 1 << 0,

    /// <summary>
    /// 审核
    /// </summary>
    ReviewPost = 1 << 1,

    /// <summary>
    /// 直接投稿
    /// </summary>
    DirectPost = 1 << 2,

    /// <summary>
    /// 普通命令
    /// </summary>
    NormalCmd = 1 << 4,

    /// <summary>
    /// 管理命令
    /// </summary>
    AdminCmd = 1 << 5,

    /// <summary>
    /// 超管命令
    /// </summary>
    SuperCmd = 1 << 6,

    /// <summary>
    /// 火星
    /// </summary>
    Mars = 1 << 7,
}
