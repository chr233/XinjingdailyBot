namespace XinjingDaily.Bot.Entry.Cache;

public sealed record UserBadgeCache
{
    private int UserId { get; init; }
    public List<string> BadgeNames { get; init; } = [];
}
