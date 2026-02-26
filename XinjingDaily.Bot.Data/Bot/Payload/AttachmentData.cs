using System.Text.Json.Serialization;
using Telegram.Bot.Types.Enums;

namespace XinjingDaily.Bot.Data.Bot.Payload;

public sealed record AttachmentData
{
    /// <summary>
    /// 主键
    /// </summary>
    [JsonPropertyName("i")]
    public int Id { get; set; }
    /// <summary>
    /// 稿件Id
    /// </summary>
    [JsonPropertyName("p")]
    public long PostId { get; set; }
    /// <summary>
    /// 文件ID
    /// </summary>
    [JsonPropertyName("i")]
    public string? FileId { get; set; }
    /// <summary>
    /// 文件名称
    /// </summary>
    public string? FileName { get; set; }
    /// <summary>
    /// 文件唯一ID
    /// </summary>
    public string? FileUniqueId { get; set; }
    /// <summary>
    /// 文件类型
    /// </summary>
    public string? MimeType { get; set; }
    /// <summary>
    /// 文件尺寸
    /// </summary>
    public long Size { get; set; }
    /// <summary>
    /// 图像高度
    /// </summary>
    public int Height { get; set; }
    /// <summary>
    /// 图像宽度
    /// </summary>
    public int Width { get; set; }
    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType Type { get; set; } = MessageType.Unknown;
}

