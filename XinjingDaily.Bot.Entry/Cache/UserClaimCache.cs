namespace XinjingDaily.Bot.Entry.Cache;

public sealed record UserClaimCache
{
    private int UserId { get; init; }
    public List<string> ClaimKeys { get; init; } = [];
}
