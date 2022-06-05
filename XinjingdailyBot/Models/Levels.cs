using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XinjingdailyBot.Models
{
    [SugarTable("level", TableDescription = "投稿记录")]
    internal sealed class Levels
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public ulong Id { get; set; }
        /// <summary>
        /// 默认设置
        /// </summary>
        public bool Default { get; set; }
        /// <summary>
        /// 等级名称
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 最小经验
        /// </summary>
        public ulong MinExp { get; set; }
        /// <summary>
        /// 最高经验
        /// </summary>
        public ulong MaxExp { get; set; }
        /// <summary>
        /// 达成人数
        /// </summary>
        public ulong ReachCount { get; set; }
    }
}
