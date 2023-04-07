using SqlSugar;

namespace XinjingdailyBot.Model.Models
{
    [SugarTable("context")]
    public sealed record Contexts
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID { get; set; }
        /// <summary>
        /// 上下文模式
        /// </summary>
        public string Mode { get; set; } = "";
    }
}
