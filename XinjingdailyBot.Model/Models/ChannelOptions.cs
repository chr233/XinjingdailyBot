using SqlSugar;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models
{
    [SugarTable("channel", TableDescription = "来源频道设定")]
    [SugarIndex("index_channel_id", nameof(ChannelID), OrderByType.Asc, true)]
    [SugarIndex("index_channel_name", nameof(ChannelName), OrderByType.Asc, false)]
    [SugarIndex("index_channel_title", nameof(ChannelTitle), OrderByType.Asc, false)]
    public sealed record ChannelOptions : BaseModel
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        /// <summary>
        /// 频道ID
        /// </summary>
        public long ChannelID { get; set; }
        /// <summary>
        /// 频道ID @
        /// </summary>
        public string ChannelName { get; set; } = "";
        /// <summary>
        /// 频道名称
        /// </summary>
        public string ChannelTitle { get; set; } = "";
        /// <summary>
        /// 封禁类型
        /// </summary>
        public EChannelOption Option { get; set; } = EChannelOption.Normal;

        public int Count { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.Now;

        public DateTime ModifyAt { get; set; } = DateTime.Now;
    }
}
