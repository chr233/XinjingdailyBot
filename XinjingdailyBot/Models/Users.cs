using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;

namespace XinjingdailyBot.Models
{
    [SugarTable("user", TableDescription = "用户表")]
    [SugarIndex("index_userid", nameof(UserID), OrderByType.Asc, true)]
    internal sealed class Users
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public ulong Id { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public ulong UserID { get; set; }
        /// <summary>
        /// 用户名@
        /// </summary>
        public string UserName { get; set; } = "";
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string userNickName { get; set; } = "";

        public bool IsBan { get; set; }
        public bool PerferAnymouse { get; set; }
        public int AcceptCount { get; set; }
        public int RejetCount { get; set; }
        public int PostCount { get; set; }
        public int ReviewCount { get; set; }
        public long Experience { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateAt { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyAt { get; set; }
    }
}
