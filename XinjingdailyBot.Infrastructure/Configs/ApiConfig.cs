using XinjingdailyBot.Infrastructure.Configs;

namespace XinjingdailyBot.Infrastructure.Options;
public sealed record ApiConfig : IXjbConfig
{
    public static string SectionName => "Api";

    public bool Debug { get; init; }
    public bool Swagger { get; init; }
    public int Port { get; init; } = 8233;
}
