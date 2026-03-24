namespace XinjingDaily.Bot.Infrastructure.Configs;

/// <summary>
/// GitHub选项
/// </summary>
public sealed record NetworkConfig
{
    public string? WebProxy { get; init; }

    /// <summary>
    /// GitHub Api地址
    /// </summary>
    public string GitHubApi { get; init; } = "https://api.github.com/";

    /// <summary>
    /// IpInfo Api地址
    /// </summary>
    public string IpInfoApi { get; init; } = "https://ipinfo.io/";

    /// <summary>
    /// IpInfo Token
    /// </summary>
    public string? IpInfoToken { get; init; }

    /// <summary>
    /// Telegram Api地址
    /// </summary>
    public string TelegramApi { get; init; } = "https://api.telegram.org/";

    public int Timeout { get; init; } = 60;
}
