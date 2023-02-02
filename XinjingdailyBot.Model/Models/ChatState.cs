using SqlSugar;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models
{
    /// <summary>
    /// 审核投票表
    /// </summary>
    [SugarTable("chat", TableDescription = "聊天模式")]
    public sealed record ChatState : BaseModel
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public long UserId { get; set; }
        /// <summary>
        /// 会话状态
        /// </summary>
        public bool InChat { get; set; }
    }
}
