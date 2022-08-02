using SqlSugar;

namespace XinjingdailyBot.Models
{
    [SugarTable("level", TableDescription = "等级组")]
    internal sealed class Levels
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }
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
    }
}
