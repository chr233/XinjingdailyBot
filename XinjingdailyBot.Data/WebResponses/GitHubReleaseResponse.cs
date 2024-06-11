using System.Text.Json.Serialization;

namespace XinjingdailyBot.Data.WebResponses;

/// <summary>
/// GitHubRelease响应实体
/// </summary>
public sealed record GitHubReleaseResponse
{
    /// <summary>
    /// html_url
    /// </summary>
    [JsonPropertyName("html_url")]
    public string Url { get; set; } = "";

    /// <summary>
    /// name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// body
    /// </summary>
    [JsonPropertyName("body")]
    public string Body { get; set; } = "";

    /// <summary>
    /// tag_name
    /// </summary>
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = "";

    /// <summary>
    /// created_at
    /// </summary>
    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = "";

    /// <summary>
    /// published_at
    /// </summary>
    [JsonPropertyName("published_at")]
    public string PublicAt { get; set; } = "";

    /// <inheritdoc cref="GitHubAssetsData"/>
    [JsonPropertyName("assets")]
    public HashSet<GitHubAssetsData> Assets { get; set; } = [];

    /// <summary>
    /// Asset数据
    /// </summary>
    public sealed record GitHubAssetsData
    {
        /// <summary>
        /// name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        /// <summary>
        /// size
        /// </summary>
        [JsonPropertyName("size")]
        public uint Size { get; set; }

        /// <summary>
        /// created_at
        /// </summary>
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = "";

        /// <summary>
        /// updated_at
        /// </summary>
        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; } = "";

        /// <summary>
        /// browser_download_url
        /// </summary>
        [JsonPropertyName("browser_download_url")]
        public string DownloadUrl { get; set; } = "";
    }
}
