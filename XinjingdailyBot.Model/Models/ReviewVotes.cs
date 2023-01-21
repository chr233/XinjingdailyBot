using SqlSugar;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models
{
    /// <summary>
    /// 审核投票表
    /// </summary>
    [SugarTable("vote", TableDescription = "审核投票")]
    public sealed record ReviewVotes : BaseModel
    {
        /// <summary>
        /// 稿件ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public int PostId { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public long UserId { get; set; }
        /// <summary>
        /// 投票结果
        /// </summary>
        public VoteOption Vote { get; set; }
    }
}
