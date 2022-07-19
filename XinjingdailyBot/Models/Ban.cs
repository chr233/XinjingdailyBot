using SqlSugar;

namespace XinjingdailyBot.Models
{
    [SugarTable("ban", TableDescription = "口球用户表")]
    [SugarIndex("index_userid", nameof(UserID), OrderByType.Asc, true)]
    public class Ban
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long UserID { get; set; }
        /// <summary>
        /// 执行封禁操作的管理员ID
        /// </summary>
        public long ExecutiveAdminID { get; set; }
        /// <summary>
        /// 封禁时间
        /// </summary>
        public DateTime BanTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 封禁理由
        /// </summary>
        public string Reason { get; set; }

    }
}
