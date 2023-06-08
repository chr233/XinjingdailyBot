using Telegram.Bot.Types.Enums;

namespace XinjingdailyBot.WebAPI.IPC.Responses;

/// <summary>
/// 投稿数据
/// </summary>
public sealed record CreatePostResponse
{
   public MessageType PostType { get; set; }
    
    public int MediaCount { get; set; }
}
