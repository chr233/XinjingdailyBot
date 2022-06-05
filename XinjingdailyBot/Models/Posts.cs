using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    [SugarIndex("index_posterid", nameof(PosterID), OrderByType.Asc)]
    [SugarIndex("index_reviewerid", nameof(ReviewerID), OrderByType.Asc)]
    internal sealed class Posts
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public ulong Id { get; set; }
        /// <summary>
        /// 原始消息会话ID
        /// </summary>
        public ulong OriginChatID { get; set; }
        /// <summary>
        /// 原始消息ID
        /// </summary>
        public ulong OriginMsgID { get; set; }
        /// <summary>
        /// 投稿控制消息ID
        /// </summary>
        public ulong ActionMsgID { get; set; }
        /// <summary>
        /// 审核群消息ID
        /// </summary>
        public ulong ReviewMsgID { get; set; }
        /// <summary>
        /// 审核群控制消息ID
        /// </summary>
        public ulong ManageMsgID { get; set; }
        /// <summary>
        /// 匿名投稿
        /// </summary>
        public bool Anymouse { get; set; }

        /// <summary>
        /// 投稿描述(过滤#标签和链接)
        /// </summary>
        public string Text { get; set; } = "";
        /// <summary>
        /// 投稿原始描述
        /// </summary>
        public string RawText { get; set; } = "";

        /// <summary>
        /// 是否为频道转发
        /// </summary>
        public bool IsFromChannel => !string.IsNullOrEmpty(ChannelID);

        /// <summary>
        /// 来源频道ID
        /// </summary>
        public string ChannelID { get; set; } = "";
        /// <summary>
        /// 来源频道名称
        /// </summary>
        public string ChannelName { get; set; } = "";
        /// <summary>
        /// 投稿状态
        /// </summary>
        public PostStatus Status { get; set; } = PostStatus.Unknown;
        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType PostType { get; set; } = MessageType.Unknown;
        /// <summary>
        /// 是否为媒体组消息
        /// </summary>
        public bool IsMediaGroup { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public byte Tags { get; set; }
        /// <summary>
        /// 拒绝原因(如果拒绝)
        /// </summary>
        public RejectReason Reason { get; set; } = RejectReason.Unknown;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateAt { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyAt { get; set; }
        /// <summary>
        /// 投稿人用户ID
        /// </summary>
        public ulong PosterID { get; set; }
        /// <summary>
        /// 审核人ID
        /// </summary>
        public ulong ReviewerID { get; set; }

        [Navigate(NavigateType.OneToMany, nameof(Models.Attachments.PostID))]
        public List<Attachments>? Attachments { get; set; }
    }
}
