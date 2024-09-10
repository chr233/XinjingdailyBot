using XinjingdailyBot.Infrastructure.Configs;

namespace XinjingdailyBot.Infrastructure.Options;
public sealed record NetworkConfig : IXjbConfig
{
    public static string SectionName => "Network";

    public string? TelegramProxy { get; set; }

    public string? TelegramEndpoint { get; set; }

    public string? MyProperty { get; set; }
}
