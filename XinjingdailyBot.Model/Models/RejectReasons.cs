using SqlSugar;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models
{
    [SugarTable("reject", TableDescription = "拒绝理由")]
    [SugarIndex("payload", nameof(Payload), OrderByType.Asc, true)]
    public sealed record RejectReasons : BaseModel
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; } = -1;

        /// <summary>
        /// 显示名称
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// CallbackData
        /// </summary>
        public string Payload { get; set; } = "";

        /// <summary>
        /// 发给用户的拒绝理由
        /// </summary>
        public string FullText { get; set; } = "";

        /// <summary>
        /// 是否计入每日投稿上限计数
        /// </summary>
        public bool IsCount { get; set; } = true;
    }
}
