using SqlSugar;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models
{
    [SugarTable("dialogue", TableDescription = "消息记录")]
    public sealed record Dialogue : BaseModel
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        /// <summary>
        /// 会话ID
        /// </summary>
        public long ChatID { get; set; }
        /// <summary>
        /// 消息ID
        /// </summary>
        public long MessageID { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserID { get; set; } = -1;

        /// <summary>
        /// 回复消息ID
        /// </summary>
        public long ReplyMessageID { get; set; } = -1;

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; } = "";

        /// <summary>
        /// 消息类型
        /// </summary>
        public string Type { get; set; } = "";
        /// <summary>
        /// 消息事件
        /// </summary>
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
