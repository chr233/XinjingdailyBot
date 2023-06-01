using SqlSugar;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models
{
    /// <summary>
    /// 用户等级表
    /// </summary>
    [SugarTable("level", TableDescription = "等级组")]
    public sealed record Levels : BaseModel
    {
        /// <summary>
        /// 主键
        /// </summary>
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
