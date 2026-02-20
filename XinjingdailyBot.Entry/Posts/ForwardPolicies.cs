using SqlSugar;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Model.Models.Posts;

/// <summary>
/// 来源频道设定
/// </summary>
[SugarTable("xjb_forward_policy", TableDescription = "转发设定")]
public sealed record ForwardPolicies : BaseModel, ICreateAt, IModifyAt
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
    [SugarColumn(IsNullable = true)]
    public string? ChannelName { get; set; }
    /// <summary>
    /// 频道名称
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Title { get; set; }

    /// <summary>
    /// 来源类型 Channel / Group
    /// </summary>
    public ChatType Type { get; set; }

    /// <summary>
    /// 禁止从该频道/群组转发消息
    /// </summary>
    public bool IsDeny { get; set; }

    /// <summary>
    /// 转发时去除来源
    /// </summary>
    public bool IsPureFrom { get; set; }


    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;
}
