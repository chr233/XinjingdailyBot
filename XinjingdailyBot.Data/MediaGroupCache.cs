using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace XinjingdailyBot.Data;

/// <summary>
/// 媒体组缓存
/// </summary>
public sealed record MediaGroupCache
{
    public int PostId { get; set; } = -1;
    public DateTime ExpireAt { get; set; }
    public string? PostText { get; set; }
    public InlineKeyboardMarkup? Keyboard { get; set; }
    public Message? ActionMessage { get; set; }

    public string? WarnMsg { get; set; }

    public MediaGroupCache(int ttl)
    {
        RenewTtl(ttl);
    }

    public void RenewTtl(int ttl)
    {
        ExpireAt = DateTime.Now.AddSeconds(ttl);
    }
}
