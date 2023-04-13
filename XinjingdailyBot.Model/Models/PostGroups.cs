using SqlSugar;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models
{
    [SugarTable("post_group", TableDescription = "媒体组稿件记录")]
    [SugarIndex("index_postid", nameof(PostID), OrderByType.Asc)]
    [SugarIndex("index_reviewerid", nameof(PublicMsgID), OrderByType.Asc)]
    public sealed record PostGroups : BaseModel
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        /// <summary>
        /// 稿件ID
        /// </summary>
        public int PostID { get; set; } = -1;
        /// <summary>
        /// 发布的消息Id
        /// </summary>
        public long PublicMsgID { get; set; } = -1;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateAt { get; set; } = DateTime.Now;
    }
}
