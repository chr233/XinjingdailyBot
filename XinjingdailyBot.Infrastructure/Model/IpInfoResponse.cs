using System.Text.Json.Serialization;

namespace XinjingdailyBot.Infrastructure.Model;

/// <summary>
/// IpInfo响应实体
/// </summary>
public sealed record IpInfoResponse
{
    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    [JsonPropertyName("hostname")]
    public string? Hostname { get; set; }

    [JsonPropertyName("anycast")]
    public bool AnyCast { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("loc")]
    public string? Loc { get; set; }

    [JsonPropertyName("org")]
    public string? Org { get; set; }

    [JsonPropertyName("postal")]
    public string? Postal { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }
}
