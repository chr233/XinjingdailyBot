namespace XinjingdailyBot.Infrastructure.Configs;

public sealed record NetworkConfig
{
    public string? TelegramProxy { get; init; }

    public string? TelegramEndpoint { get; init; }

    public string? HttpProxy { get; init; }
}
