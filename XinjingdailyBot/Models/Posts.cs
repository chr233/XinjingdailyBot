using SqlSugar;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;

namespace XinjingdailyBot.Models
{
    [SugarTable("post", TableDescription = "投稿记录")]
    [SugarIndex("index_origin_cid", nameof(OriginChatID), OrderByType.Asc)]
    [SugarIndex("index_origin_mid", nameof(OriginMsgID), OrderByType.Asc)]
    [SugarIndex("index_action_mid", nameof(ActionMsgID), OrderByType.Asc)]
    [SugarIndex("index_review_mid", nameof(ReviewMsgID), OrderByType.Asc)]
    [SugarIndex("index_manage_mid", nameof(ManageMsgID), OrderByType.Asc)]
    [SugarIndex("index_review_mid_manage_mid", nameof(ReviewMsgID), OrderByType.Asc, nameof(ManageMsgID), OrderByType.Asc)]
    [SugarIndex("index_media_group_id", nameof(MediaGroupID), OrderByType.Asc)]
    [SugarIndex("index_posterid", nameof(PosterUID), OrderByType.Asc)]
    [SugarIndex("index_reviewerid", nameof(ReviewerUID), OrderByType.Asc)]
    [SugarIndex("index_status_modifyat", nameof(Status), OrderByType.Asc, nameof(ModifyAt), OrderByType.Asc)]
    internal sealed class Posts
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }
        /// <summary>
        /// 原始消息会话ID
        /// </summary>
        public long OriginChatID { get; set; } = -1;
        /// <summary>
        /// 原始消息ID
        /// </summary>
        public long OriginMsgID { get; set; } = -1;
        /// <summary>
        /// 投稿控制消息ID
        /// </summary>
        public long ActionMsgID { get; set; } = -1;
        /// <summary>
        /// 审核群消息ID
        /// </summary>
        public long ReviewMsgID { get; set; } = -1;
        /// <summary>
        /// 审核群控制消息ID
        /// </summary>
        public long ManageMsgID { get; set; } = -1;
        /// <summary>
        /// 发布的消息Id
        /// </summary>
        public long PublicMsgID { get; set; } = -1;

        /// <summary>
        /// 是否为直接投稿
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsDirectPost => ManageMsgID == ActionMsgID;

        /// <summary>
        /// 匿名投稿
        /// </summary>
        public bool Anymouse { get; set; }

        /// <summary>
        /// 投稿描述(过滤#标签和链接)
        /// </summary>
        [SugarColumn(ColumnDataType = "Nvarchar(2000)")]
        public string Text { get; set; } = "";
        /// <summary>
        /// 投稿原始描述
        /// </summary>
        [SugarColumn(ColumnDataType = "Nvarchar(2000)")]
        public string RawText { get; set; } = "";

        /// <summary>
        /// 是否为频道转发
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsFromChannel => !string.IsNullOrEmpty(ChannelName);

        /// <summary>
        /// 来源频道ID
        /// </summary>
        public string ChannelName { get; set; } = "";
        /// <summary>
        /// 来源频道名称
        /// </summary>
        public string ChannelTitle { get; set; } = "";
        /// <summary>
        /// 投稿状态
        /// </summary>
        public PostStatus Status { get; set; } = PostStatus.Unknown;
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
        public bool IsMediaGroup => !string.IsNullOrEmpty(MediaGroupID);
        /// <summary>
        /// 媒体组ID
        /// </summary>
        public string MediaGroupID { get; set; } = "";
        /// <summary>
        /// 标签
        /// </summary>
        public BuildInTags Tags { get; set; }
        /// <summary>
        /// 拒绝原因(如果拒绝)
        /// </summary>
        public RejectReason Reason { get; set; } = RejectReason.NotReject;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateAt { get; set; } = DateTime.Now;
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyAt { get; set; } = DateTime.Now;
        /// <summary>
        /// 投稿人用户ID
        /// </summary>
        public long PosterUID { get; set; } = -1;
        /// <summary>
        /// 审核人用户ID
        /// </summary>
        public long ReviewerUID { get; set; } = -1;
    }
}
