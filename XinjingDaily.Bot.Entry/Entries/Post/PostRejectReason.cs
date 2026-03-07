using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Posts;

/// <summary>
/// 投稿拒绝理由表
/// </summary>
[SugarTable("post_reject_reason", TableDescription = "投稿拒绝理由")]
public sealed record PostRejectReason : ICreateAt, IModifyAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int Id { get; set; }

    /// <summary>
    /// 排序优先级, 数字越小优先展示
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 拒绝理由
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Reason { get; set; }

    public bool IsCount { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.MinValue;
}
