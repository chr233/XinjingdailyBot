using SqlSugar;

namespace XinjingdailyBot.Models
{
    [SugarTable("channel", TableDescription = "来源频道设定")]
    [SugarIndex("index_channel_id", nameof(ChannelID), OrderByType.Asc, true)]
    internal sealed class Channels
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }
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
        public ChannelOption Option { get; set; } = ChannelOption.Normal;

        public DateTime CreateAt { get; set; } = DateTime.Now;

        public DateTime ModifyAt { get; set; } = DateTime.Now;
    }
}
