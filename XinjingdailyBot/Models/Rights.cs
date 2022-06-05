using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XinjingdailyBot.Enums;

namespace XinjingdailyBot.Models
{
    [SugarTable("right", TableDescription = "用户权限")]
    internal sealed class Rights
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public ulong Id { get; set; }
        /// <summary>
        /// 默认设置
        /// </summary>
        public bool Default { get; set; }
        /// <summary>
        /// 权限名
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 权限设置
        /// </summary>
        public UserRights Right { get; set; }
    }
}
