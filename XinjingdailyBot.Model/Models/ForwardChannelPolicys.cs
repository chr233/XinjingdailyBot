using SqlSugar;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Model.Models;

/// <summary>
/// 来源频道设定
/// </summary>
[SugarTable("channel_option", TableDescription = "投稿来源频道设定")]
[SugarIndex("co_channel_id", nameof(ChatId), OrderByType.Asc, true)]
[SugarIndex("index_channel_name", nameof(ChannelName), OrderByType.Asc, false)]
public sealed record ForwardChannelPolicys : BaseModel, ICreateAt, IModifyAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    /// <summary>
    /// 频道ID
    /// </summary>
    public long ChatId { get; set; }
    /// <summary>
    /// 频道ID @
    /// </summary>
    public string ChannelName { get; set; } = "";
    /// <summary>
    /// 频道名称
    /// </summary>
    public string ChannelTitle { get; set; } = "";
    /// <summary>
    /// 封禁类型
    /// </summary>
    public EChannelOption Option { get; set; } = EChannelOption.Normal;

    /// <summary>
    /// 频道引用计数
    /// </summary>
    public int Count { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;
}
