using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;
using XinjingDaily.Bot.Infrastructure.Enums;

namespace XinjingDaily.Bot.Entry.Entries.Posts;

/// <summary>
/// 发布频道设定表
/// </summary>
[SugarTable("post_channel", TableDescription = "发布频道设定")]
[SugarIndex("i_post_channel_setting_chat_id", nameof(ChatId), OrderByType.Asc, true)]
public sealed record PostChannelSetting : ICreateAt, IModifyAt, IEnabled
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

    public int? MessageThredId { get; set; }

    /// <summary>
    /// 优先级, 数字越小越靠前
    /// </summary>
    public int Order { get; set; }

    /// <inheritdoc cref="IEnabled"/>
    public bool IsEnabled { get; set; }

    public EPostChannelType Type { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.MinValue;
}
