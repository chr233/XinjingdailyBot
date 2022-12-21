using SqlSugar;
using XinjingdailyBot.Model.Enums.Base;

namespace XinjingdailyBot.Model.Models
{
    [SugarTable("vip", TableDescription = "高级用户开通记录")]
    [SugarIndex("index_userid", nameof(UserID), OrderByType.Asc, true)]
    [SugarIndex("index_userid_active", nameof(UserID), OrderByType.Asc, nameof(Active), OrderByType.Desc, true)]
    public sealed record VipRecords : BaseModel
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// 记录生效中
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpireAt { get; set; } = DateTime.MaxValue;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateAt { get; set; } = DateTime.Now;
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyAt { get; set; } = DateTime.Now;
    }
}
