namespace XinjingdailyBot.Infrastructure.Configs;
public sealed record BotConfig : IXjbConfig
{
    public static string SectionName => "Bot";

    public string? BotToken { get; init; }
    public List<string>? BotTokens { get; init; }
}
