using SqlSugar;

namespace XinjingdailyBot.Models
{
    [SugarTable("cmd", TableDescription = "命令回调")]
    [SugarIndex("index_cid", nameof(ChatID), OrderByType.Asc)]
    [SugarIndex("index_mid", nameof(MessageID), OrderByType.Asc)]
    internal class CmdActions
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
        /// 发起人UID
        /// </summary>
        public long OperatorUID { get; set; }
        /// <summary>
        /// 命令原文
        /// </summary>
        public string Command { get; set; } = "";
        /// <summary>
        /// 目标用户UID
        /// </summary>
        public long TargetUserID { get; set; } = -1;

        /// <summary>
        /// 命令执行是否结束
        /// </summary>
        public bool IsDone { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime ExecTime { get; set; } = DateTime.Now;
    }
}
