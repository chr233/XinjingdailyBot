using DsNext.Infrastructure.Options;

namespace XinjingdailyBot.Infrastructure.Options;
public sealed record BotConfig : IXjbConfig
{
    public static string? SectionName => "Bot";
}
