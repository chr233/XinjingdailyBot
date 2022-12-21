using SqlSugar;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models
{
    /// <summary>
    /// 用户表, 储存所有用户的基本信息, 权限设定, 以及投稿信息统计
    /// </summary>
    [SugarTable("user", TableDescription = "用户表")]
    [SugarIndex("index_userid", nameof(UserID), OrderByType.Asc, true)]
    [SugarIndex("index_username", nameof(UserName), OrderByType.Asc)]
    public sealed record Users : BaseModel
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserID { get; set; }
        /// <summary>
        /// 用户名@
        /// </summary>
        public string UserName { get; set; } = "";
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        /// <summary>
        /// 用户昵称
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string UserNick => string.IsNullOrEmpty(LastName) ? FirstName : $"{FirstName} {LastName}";
        /// <summary>
        /// 是否封禁
        /// </summary>
        public bool IsBan { get; set; } = false;
        /// <summary>
        /// 是否为Bot
        /// </summary>
        public bool IsBot { get; set; }
        /// <summary>
        /// 是否为高级用户
        /// </summary>
        public bool IsVip { get; set; }

        /// <summary>
        /// 默认开启匿名模式
        /// </summary>
        public bool PreferAnymouse { get; set; }
        /// <summary>
        /// 是否开启通知
        /// </summary>
        public bool Notification { get; set; } = true;
        /// <summary>
        /// 通过的稿件数量
        /// </summary>
        public int AcceptCount { get; set; }
        /// <summary>
        /// 被拒绝的稿件数量
        /// </summary>
        public int RejetCount { get; set; }
        /// <summary>
        /// 过期未被审核的稿件数量(统计时总投稿需要减去此字段)
        /// </summary>
        public int ExpiredPostCount { get; set; }

        /// <summary>
        /// 投稿数量
        /// </summary>
        public int PostCount { get; set; }
        /// <summary>
        /// 审核数量
        /// </summary>
        public int ReviewCount { get; set; }
        /// <summary>
        /// 经验
        /// </summary>
        public long Experience { get; set; }
        /// <summary>
        /// 用户等级
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 用户权限
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public UserRights Right { get; set; } = UserRights.None;
        /// <summary>
        /// 用户组ID
        /// </summary>
        public int GroupID { get; set; }
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

        public override string ToString()
        {
            if (string.IsNullOrEmpty(UserName))
            {
                return $"{UserNick}(#{UserID})";
            }
            else
            {
                return $"{UserNick}(@{UserName})";
            }
        }
    }
}
