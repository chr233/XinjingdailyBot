using System.Text.Json.Serialization;



namespace XinjingdailyBot.Infrastructure.Model
{
    /// <summary>
    /// GitHubRelease响应实体
    /// </summary>
    public sealed record GitHubReleaseResponse
    {
        [JsonPropertyName("html_url")]
        public string Url { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("body")]
        public string Body { get; set; } = "";

        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = "";

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = "";

        [JsonPropertyName("published_at")]
        public string PublicAt { get; set; } = "";

        [JsonPropertyName("assets")]
        public HashSet<GitHubAssetsData> Assets { get; set; } = new();

        public sealed record GitHubAssetsData
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = "";

            [JsonPropertyName("size")]
            public uint Size { get; set; }

            [JsonPropertyName("created_at")]
            public string CreatedAt { get; set; } = "";

            [JsonPropertyName("updated_at")]
            public string UpdatedAt { get; set; } = "";

            [JsonPropertyName("browser_download_url")]
            public string DownloadUrl { get; set; } = "";
        }
    }
}
