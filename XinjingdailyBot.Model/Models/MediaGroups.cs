using SqlSugar;
using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Model.Models
{
    [SugarTable("post_group", TableDescription = "媒体组稿件记录")]
    [SugarIndex("index_msgid", nameof(ChatID), OrderByType.Asc, nameof(MessageID), OrderByType.Asc)]
    [SugarIndex("index_groupid", nameof(MediaGroupID), OrderByType.Asc)]
    public sealed record MediaGroups : BaseModel, ICreateAt
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        /// <summary>
        /// 聊天ID
        /// </summary>
        public long ChatID { get; set; } = -1;
        /// <summary>
        /// 发布的消息Id
        /// </summary>
        public long MessageID { get; set; } = -1;
        /// <summary>
        /// 稿件ID
        /// </summary>
        public string MediaGroupID { get; set; } = "";
        /// <inheritdoc cref="ICreateAt.CreateAt"/>
        public DateTime CreateAt { get; set; } = DateTime.Now;
    }
}
