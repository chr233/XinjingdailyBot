using Telegram.Bot.Types.Enums;

namespace XinjingdailyBot.WebAPI.IPC.Responses;

/// <summary>
/// 投稿数据
/// </summary>
public sealed record CreatePostResponse
{
    /// <summary>
    /// 稿件类型
    /// </summary>
    public MessageType PostType { get; set; }
    /// <summary>
    /// 媒体数量
    /// </summary>
    public int MediaCount { get; set; }
}
