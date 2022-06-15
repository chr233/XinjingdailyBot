using SqlSugar;

namespace XinjingdailyBot.Models
{
    [SugarTable("cmd", TableDescription = "命令回调")]
    [SugarIndex("index_cid", nameof(ChatID), OrderByType.Asc)]
    [SugarIndex("index_mid", nameof(MessageID), OrderByType.Asc)]
    internal class CmdRecord
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
        /// 用户ID(UserID列)
        /// </summary>
        public long UserID { get; set; }
        /// <summary>
        /// 命令
        /// </summary>
        public string Command { get; set; } = "";
        /// <summary>
        /// 目标用户
        /// </summary>
        public long TargetUserID { get; set; } = -1;
    }
}
