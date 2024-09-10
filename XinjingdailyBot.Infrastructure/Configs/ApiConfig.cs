using XinjingdailyBot.Infrastructure.Configs;

namespace XinjingdailyBot.Infrastructure.Options;
public sealed record ApiConfig : IXjbConfig
{
    public static string SectionName => "Api";

    public bool Debug { get; set; }
    public bool Swagger { get; set; }
    public int Port { get; set; } = 8233;
}
