using SqlSugar;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models
{
    [SugarTable("reject", TableDescription = "拒绝理由")]
    public sealed record RejectReason : BaseModel
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }
        
        /// <summary>
        /// 显示名称
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 发给用户的拒绝理由
        /// </summary>
        public string FullText { get; set; } = "";
    }
}
