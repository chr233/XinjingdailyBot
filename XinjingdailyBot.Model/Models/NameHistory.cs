using SqlSugar;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models
{
    /// <summary>
    /// 用户等级表
    /// </summary>
    [SugarTable("name_history", TableDescription = "等级组")]
    public sealed record NameHistory : BaseModel
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }
        /// <summary>
        /// 用户UID
        /// </summary>
        public int UId { get; set; }
        /// <summary>
        /// 用户昵称 姓
        /// </summary>
        public string FirstName { get; set; } = "";
        /// <summary>
        /// 用户昵称 名
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// 添加日期
        /// </summary>
        public DateTime CreateAt { get; set; }
    }
}
