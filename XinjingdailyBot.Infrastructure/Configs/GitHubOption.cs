namespace XinjingdailyBot.Infrastructure.Configs;

/// <summary>
/// GitHub选项
/// </summary>
public sealed record GitHubOption
{
    /// <summary>
    /// Github Api地址
    /// </summary>
    public string? BaseUrl { get; set; }
}
