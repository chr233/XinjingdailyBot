using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Posts;

/// <summary>
/// 投稿标签表
/// </summary>
[SugarTable("post_tag", TableDescription = "投稿标签")]
[SugarIndex("i_post_tag_payload", nameof(Payload), OrderByType.Asc, true)]
public sealed record PostTag : ICreateAt, IModifyAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int Id { get; set; }
    /// <summary>
    /// 位
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public int Seg { get; set; }
    /// <summary>
    /// 标签名
    /// </summary>
    public string Name { get; set; } = "";
    /// <summary>
    /// CallbackData
    /// </summary>
    [SugarColumn(Length = 50)]
    public string Payload { get; set; } = "";
    /// <summary>
    /// 启用文本
    /// </summary> 
    public string OnText { get; set; } = "";
    /// <summary>
    /// 禁用文本
    /// </summary>
    public string OffText { get; set; } = "";
    /// <summary>
    /// 标签文本
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? HashTag { get; set; }
    /// <summary>
    /// 自动识别关键字, | 分隔
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? KeyWords { get; set; }
    /// <summary>
    /// 警告文本, 带有此Tag的投稿会在发布时提前发送警告
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? WarnText { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;
    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;
}
