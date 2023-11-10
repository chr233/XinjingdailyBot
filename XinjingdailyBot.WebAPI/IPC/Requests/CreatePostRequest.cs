using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Telegram.Bot.Types.Enums;

namespace XinjingdailyBot.WebAPI.IPC.Requests;

/// <summary>
/// 投稿数据
/// </summary>
public sealed record CreatePostRequest
{
    /// <summary>
    /// 文字描述
    /// </summary>
    [MaxLength(2000)]
    [DefaultValue("")]
    public string? Text { get; set; }
    /// <summary>
    /// 多媒体文件
    /// </summary>
    public IFormFileCollection? Media { get; set; }
    /// <summary>
    /// 多媒体文件名
    /// </summary>
    public IList<string>? MediaNames { get; set; }
    /// <summary>
    /// 消息类型
    /// </summary>
    [DefaultValue(MessageType.Unknown)]
    public MessageType PostType { get; set; } = MessageType.Unknown;
    /// <summary>
    /// 是否启用遮罩
    /// </summary>
    [DefaultValue(false)]
    public bool HasSpoiler { get; set; }
    /// <summary>
    /// 频道ID
    /// </summary>
    [DefaultValue(0)]
    public long ChannelID { get; set; } = 0;
    /// <summary>
    /// 频道ID @
    /// </summary>
    [DefaultValue("")]
    public string? ChannelName { get; set; }
    /// <summary>
    /// 频道名称
    /// </summary>
    [DefaultValue("")]
    public string? ChannelTitle { get; set; }
    /// <summary>
    /// 转发消息ID
    /// </summary>
    [DefaultValue(0)]
    public long ChannelMsgID { get; set; } = 0;
}


