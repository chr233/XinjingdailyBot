using SqlSugar;

namespace XinjingdailyBot.Models
{
    [SugarTable("ban", TableDescription = "用户封禁记录户表")]
    [SugarIndex("index_userid", nameof(UserID), OrderByType.Asc, true)]
    [SugarIndex("index_operatorid", nameof(OperatorUID), OrderByType.Asc)]
    public class BanRecords
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserID { get; set; }
        /// <summary>
        /// 执行封禁操作的管理员ID
        /// </summary>
        public long OperatorUID { get; set; }
        /// <summary>
        /// 是否封禁 true: 封禁, false: 解封
        /// </summary>
        public bool IsBan { get; set; } = true;
        /// <summary>
        /// 封禁时间
        /// </summary>
        public DateTime BanTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 封禁理由
        /// </summary>
        public string Reason { get; set; } = "";
    }
}
