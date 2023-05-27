using SqlSugar;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models
{
    /// <summary>
    /// 用户表, 储存所有用户的基本信息, 权限设定, 以及投稿信息统计
    /// </summary>
    [SugarTable("user_token", TableDescription = "用户密钥表")]
    [SugarIndex("index_userid", nameof(UserID), OrderByType.Asc, true)]
    [SugarIndex("index_username", nameof(UserName), OrderByType.Asc)]
    [SugarIndex("index_token", nameof(APIToken), OrderByType.Asc, false)]
    public sealed record UserToken : BaseModel
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserID { get; set; }
       
        /// <summary>
        /// 私聊ChatID, 默认 -1;
        /// </summary>
        public long PrivateChatID { get; set; } = -1;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateAt { get; set; } = DateTime.Now;
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyAt { get; set; } = DateTime.Now;

        /// <summary>
        /// API Token
        /// </summary>
        public Guid? APIToken { get; set; }

    }
}
