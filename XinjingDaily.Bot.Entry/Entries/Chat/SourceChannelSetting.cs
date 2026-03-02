using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Channel;

/// <summary>
/// 来源频道设定
/// </summary>
[SugarTable("source_channel_setting", TableDescription = "来源频道设定")]
[SugarIndex("i_source_channel_setting_chat_id", nameof(ChatId), OrderByType.Asc, true)]
public sealed record SourceChannelSetting : ICreateAt, IModifyAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// ChatInfo主键
    /// </summary>
    public int ChatId { get; set; }

    /// <summary>
    /// 是否自动拒绝频道投稿
    /// </summary>
    public bool IsAutoReject { get; set; }

    /// <summary>
    /// 是否去除消息来源
    /// </summary>
    public bool IsPureSource { get; set; }

    /// <summary>
    /// 是否自动设置剧透遮罩
    /// </summary>
    public bool IsSpoiler { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.MinValue;
}
