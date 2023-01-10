using SqlSugar;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models
{
    /// <summary>
    /// 储存命令执行记录, 也用于查找用户
    /// </summary>
    [SugarTable("cmd", TableDescription = "命令回调")]
    [SugarIndex("index_cid", nameof(ChatID), OrderByType.Asc)]
    [SugarIndex("index_mid", nameof(MessageID), OrderByType.Asc)]
    [SugarIndex("index_uid", nameof(UserID), OrderByType.Asc)]
    public sealed record CmdRecords : BaseModel
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        /// <summary>
        /// 原会话ID
        /// </summary>
        public long ChatID { get; set; }
        /// <summary>
        /// 原消息ID
        /// </summary>
        public long MessageID { get; set; }
        /// <summary>
        /// 调用者UID
        /// </summary>
        public long UserID { get; set; }
        /// <summary>
        /// 命令原文
        /// </summary>
        public string Command { get; set; } = "";
        /// <summary>
        /// 错误消息
        /// </summary>
        [SugarColumn(Length = 2000)]
        public string Exception { get; set; } = "";

        /// <summary>
        /// 是否为Query命令
        /// </summary>
        public bool IsQuery { get; set; }
        /// <summary>
        /// 命令成功执行
        /// </summary>
        public bool Handled { get; set; }
        /// <summary>
        /// 命令执行出错
        /// </summary>
        public bool Error { get; set; }

        /// <summary>
        /// 记录命令调用时间
        /// </summary>
        public DateTime ExecuteAt { get; set; } = DateTime.Now;
    }
}
