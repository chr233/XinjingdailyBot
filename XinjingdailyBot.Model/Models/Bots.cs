using SqlSugar;
using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Model.Models;

/// <summary>
/// 新的稿件表
/// </summary>
[SugarTable("new_post", TableDescription = "投稿记录")]
[SugarIndex("index_origin", nameof(OriginChatID), OrderByType.Asc, nameof(OriginMsgID), OrderByType.Asc)]
[SugarIndex("index_originaction", nameof(OriginActionChatID), OrderByType.Asc, nameof(OriginActionMsgID), OrderByType.Asc)]
[SugarIndex("index_review", nameof(ReviewChatID), OrderByType.Asc, nameof(ReviewMsgID), OrderByType.Asc)]
[SugarIndex("index_reviewaction", nameof(ReviewActionChatID), OrderByType.Asc, nameof(ReviewActionMsgID), OrderByType.Asc)]
[SugarIndex("index_origin_media_group_id", nameof(OriginMediaGroupID), OrderByType.Asc)]
[SugarIndex("index_review_media_group_id", nameof(ReviewMediaGroupID), OrderByType.Asc)]
[SugarIndex("index_post_media_group_id", nameof(PublishMediaGroupID), OrderByType.Asc)]
[SugarIndex("index_posterid", nameof(PosterUID), OrderByType.Asc)]
[SugarIndex("index_reviewerid", nameof(ReviewerUID), OrderByType.Asc)]
[SugarIndex("index_status_modifyat", nameof(Status), OrderByType.Asc, nameof(ModifyAt), OrderByType.Asc)]
public sealed record Bots : BaseModel, IModifyAt, ICreateAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(Length = 64)]
    public string? BotToken { get; set; }

    public int MyProperty { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;
    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;
}
