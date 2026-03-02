namespace XinjingDaily.Bot.Infrastructure.Enums;

/// <summary>
/// 命令可用会话场景
/// </summary>
public enum ECommandScope : byte
{
    /// <summary>
    /// 所有场景
    /// </summary>
    All = 0,

    /// <summary>
    /// 私聊
    /// </summary>
    Private = 1 << 0,

    /// <summary>
    /// 群聊
    /// </summary>
    Group = 1 << 1,
}
