using XinjingdailyBot.Infrastructure;

namespace XinjingdailyBot.Controllers.Responses;
public sealed record AboutResponse
{
    public string Application { get; init; }
    public string? Version { get; init; }
    public string? Framework { get; init; }
    public string? Company { get; init; }
    public string? Description { get; init; }
    public string? Copyright { get; init; }

    public AboutResponse()
    {
        Application = nameof(SteamLevelupCsharp);
        Version = BuildInfo.Version;
        Framework = BuildInfo.FrameworkName;
        Company = BuildInfo.Company;
        Description = BuildInfo.Description;
        Copyright = BuildInfo.Copyright;
    }
}
