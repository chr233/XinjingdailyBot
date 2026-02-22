namespace XinjingdailyBot.Entry.Cache;

public sealed record UserPermissionCache
{
    private int UserId { get; init; }
    public List<string> Permission { get; init; }
}
