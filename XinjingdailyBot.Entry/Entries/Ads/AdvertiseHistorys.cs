using SqlSugar;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Entry.Entries.Ads;

/// <summary>
/// 广告消息表
/// </summary>
[SugarTable("advertise_history", TableDescription = "广告消息")]
public sealed record AdvertiseHistorys : IModifyAt, ICreateAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    /// <summary>
    /// 广告ID
    /// </summary>
    public int AdId { get; set; }
    /// <summary>
    /// 原会话ID
    /// </summary>
    public long ChatId { get; set; }
    /// <summary>
    /// 原消息ID
    /// </summary>
    [SugarColumn(IsJson = true)]
    public List<int> MessageIds { get; set; } = [];
    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool IsPinTop { get; set; }
    /// <summary>
    /// 是否置底
    /// </summary>
    public bool IsPinBottom { get; set; }
    /// <summary>
    /// 是否被删除
    /// </summary>
    public bool IsDeleted { get; set; }
    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;
    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;
}
