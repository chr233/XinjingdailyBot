using SqlSugar;
using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Model.Models;

/// <summary>
/// 来源频道设定
/// </summary>
[SugarTable("channel", TableDescription = "频道")]
public sealed record Channels : BaseModel, ICreateAt, IModifyAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    /// <summary>
    /// 频道ID
    /// </summary>
    public long ChannelId { get; set; }
    /// <summary>
    /// 频道ID @
    /// </summary>
    public string ChannelName { get; set; } = "";
    /// <summary>
    /// 频道名称
    /// </summary>
    public string ChannelTitle { get; set; } = "";





    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;
}
