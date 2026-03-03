using SqlSugar;
using Telegram.Bot.Types.Enums;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.History.Message;

[SugarTable("message_attachment", TableDescription = "聊天附件")]
public sealed record MessageAttachment : ICreateAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 文件ID
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? FileId { get; set; }
    /// <summary>
    /// 文件名称
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? FileName { get; set; }
    /// <summary>
    /// 文件唯一ID
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? FileUniqueId { get; set; }
    /// <summary>
    /// 文件类型
    /// </summary>
    [SugarColumn(IsNullable = true)]
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

    public DateTime CreateAt { get; set; }
    public DateTime ModifyAt { get; set; }
}
