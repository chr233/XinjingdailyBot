using SqlSugar;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models
{
    [SugarTable("ad", TableDescription = "广告投放")]
    public sealed record Advertises : BaseModel
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        /// <summary>
        /// 原会话ID
        /// </summary>
        public long ChatID { get; set; }
        /// <summary>
        /// 原消息ID
        /// </summary>
        public long MessageID { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// 是否自动置顶消息
        /// </summary>
        public bool PinMessage { get; set; }

        /// <summary>
        /// 广告发布位置
        /// </summary>
        public AdMode Mode { get; set; } = AdMode.None;
        
        /// <summary>
        /// 展示权重, 数值越大概率越高, 0为不展示
        /// </summary>
        public byte Weight { get; set; }

        /// <summary>
        /// 上次发布时间
        /// </summary>
        public DateTime LastPostAt { get; set; } = DateTime.MinValue;

        /// <summary>
        /// 广告展示次数
        /// </summary>
        public uint ShowCount { get; set; }
        /// <summary>
        /// 最大展示次数, 当次数值不为0且展示次数大于等于该值时自动禁用
        /// </summary>
        public uint MaxShowCount { get; set; }

        /// <summary>
        /// 过期时间, 系统时间大于过期时间自动禁用
        /// </summary>
        public DateTime ExpireAt { get; set; } = DateTime.MaxValue;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateAt { get; set; } = DateTime.Now;
    }
}
