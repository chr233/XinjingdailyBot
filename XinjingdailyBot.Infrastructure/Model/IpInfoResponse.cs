using System.Text.Json.Serialization;

namespace XinjingdailyBot.Infrastructure.Model;

/// <summary>
/// IpInfo响应实体
/// </summary>
public sealed record IpInfoResponse
{
    /// <summary>
    /// ip
    /// </summary>
    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    /// <summary>
    /// hostname
    /// </summary>
    [JsonPropertyName("hostname")]
    public string? Hostname { get; set; }

    /// <summary>
    /// anycast
    /// </summary>
    [JsonPropertyName("anycast")]
    public bool AnyCast { get; set; }

    /// <summary>
    /// city
    /// </summary>
    [JsonPropertyName("city")]
    public string? City { get; set; }

    /// <summary>
    /// region
    /// </summary>
    [JsonPropertyName("region")]
    public string? Region { get; set; }

    /// <summary>
    /// country
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    /// <summary>
    /// loc
    /// </summary>
    [JsonPropertyName("loc")]
    public string? Loc { get; set; }

    /// <summary>
    /// org
    /// </summary>
    [JsonPropertyName("org")]
    public string? Org { get; set; }

    /// <summary>
    /// postal
    /// </summary>
    [JsonPropertyName("postal")]
    public string? Postal { get; set; }

    /// <summary>
    /// timezone
    /// </summary>
    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }
}
