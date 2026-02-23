using SqlSugar;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Entry.Entries.Posts;

/// <summary>
/// 新的稿件表
/// </summary>
[SugarTable("post", TableDescription = "投稿记录")]
public sealed record Posts : IModifyAt, ICreateAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    /// <summary>
    /// 原始消息会话ID
    /// </summary>
    public long OriginChatId { get; set; } = -1;
    /// <summary>
    /// 原始消息ID
    /// </summary>
    [SugarColumn(IsJson = true)]
    public List<long> OriginMessageIds { get; set; } = [];

    /// <summary>
    /// 投稿控制消息会话ID
    /// </summary>
    public long OriginActionChatId { get; set; } = -1;

    /// <summary>
    /// 投稿控制消息ID
    /// </summary>
    public long OriginActionMessageId { get; set; } = -1;

    /// <summary>
    /// 审核消息会话ID
    /// </summary>
    public long ReviewChatId { get; set; } = -1;
    /// <summary>
    /// 审核群消息ID
    /// </summary>
    public long ReviewMessageId { get; set; } = -1;

    /// <summary>
    /// 审核群控制消息会话ID
    /// </summary>
    public long ReviewActionChatId { get; set; } = -1;
    /// <summary>
    /// 审核群控制消息ID
    /// </summary>
    public long ReviewActionMsgId { get; set; } = -1;

    /// <summary>
    /// 发布频道或拒绝频道的消息Id
    /// </summary>
    [SugarColumn(IsJson = true)]
    public List<long> PublicMessageIds { get; set; } = [];

    /// <summary>
    /// 警告消息Id
    /// </summary>
    public long WarnMessageId { get; set; } = -1;

    /// <summary>
    /// 是否为直接投稿
    /// </summary>
    public bool IsDirectPost { get; set; }

    /// <summary>
    /// 匿名投稿
    /// </summary>
    public EAnonymousType Anonymous { get; set; }

    /// <summary>
    /// 投稿描述(过滤#标签和链接)
    /// </summary>
    [SugarColumn(Length = 2000)]
    public string Text { get; set; } = "";
    /// <summary>
    /// 投稿原始描述
    /// </summary>
    [SugarColumn(Length = 2000)]
    public string RawText { get; set; } = "";

    /// <summary>
    /// 来源频道ID
    /// </summary>
    public long ForwardChannelId { get; set; } = -1;
    /// <summary>
    /// 来源频道链接
    /// </summary>
    public long ForwardChannelMessageId { get; set; } = -1;
    /// <summary>
    /// 是否为频道转发
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public bool IsFromChannel => ForwardChannelId != -1;

    /// <summary>
    /// 投稿状态
    /// </summary>
    public EPostStatus Status { get; set; } = EPostStatus.Unknown;
    /// <summary>
    /// 是否有附件
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public bool HasAttachments => PostType != MessageType.Text;
    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType PostType { get; set; } = MessageType.Unknown;
    /// <summary>
    /// 是否为媒体组消息
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public bool IsMediaGroup => !string.IsNullOrEmpty(OriginMediaGroupId);
    /// <summary>
    /// 原始媒体组ID
    /// </summary>
    public string OriginMediaGroupId { get; set; } = "";
    /// <summary>
    /// 审核消息媒体组ID
    /// </summary>
    public string ReviewMediaGroupId { get; set; } = "";
    /// <summary>
    /// 发布频道或者拒绝频道的媒体组ID
    /// </summary>
    public string PublishMediaGroupId { get; set; } = "";
    /// <summary>
    /// 稿件标签
    /// </summary>
    public int Tags { get; set; }
    /// <summary>
    /// 是否启用遮罩
    /// </summary>
    public bool HasSpoiler { get; set; }

    /// <summary>
    /// 是否允许遮罩
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public bool CanSpoiler => PostType == MessageType.Photo || PostType == MessageType.Video || PostType == MessageType.Animation;

    /// <summary>
    /// 拒绝原因(如果拒绝)
    /// </summary>
    public string RejectReason { get; set; } = "";
    /// <summary>
    /// 拒绝理由计数
    /// </summary>
    public bool CountReject { get; set; }

    /// <summary>
    /// 投稿人用户ID
    /// </summary>
    public long PosterUId { get; set; } = -1;
    /// <summary>
    /// 审核人用户ID
    /// </summary>
    public long ReviewerUId { get; set; } = -1;

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;
    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;
}
