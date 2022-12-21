using SqlSugar;
using XinjingdailyBot.Model.Enums.Base;

namespace XinjingdailyBot.Model.Models
{
    [SugarTable("answer", TableDescription = "关键词回复规则")]
    public sealed record Answers : BaseModel
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }
        /// <summary>
        /// 原会话ID
        /// </summary>
        public long ChatID { get; set; }
        /// <summary>
        /// 原消息ID
        /// </summary>
        public long MessageID { get; set; }
        /// <summary>
        /// 展示概率%
        /// </summary>
        public uint Chance { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool Enable => ExpireAt > DateTime.Now;
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpireAt { get; set; } = DateTime.Now;

        public DateTime CreateAt { get; set; } = DateTime.Now;

        public DateTime ModifyAt { get; set; } = DateTime.Now;
    }
}
