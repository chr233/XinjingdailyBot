using XinjingdailyBot.Infrastructure.Configs;

namespace XinjingdailyBot.Infrastructure.Options;
public sealed record NetworkConfig : IXjbConfig
{
    public static string SectionName => "Network";

    public string? TelegramProxy { get; init; }

    public string? TelegramEndpoint { get; init; }

    public string? HttpProxy { get; init; }
}
