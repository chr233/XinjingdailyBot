using SqlSugar;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models
{
    [SugarTable("tag", TableDescription = "投稿标签")]
    public sealed record Tags : BaseModel
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }
        /// <summary>
        /// 标签名
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 启用文本
        /// </summary>
        public string OnText { get; set; } = "";
        /// <summary>
        /// 禁用文本
        /// </summary>
        public string OffText { get; set; } = "";
        /// <summary>
        /// 标签文本
        /// </summary>
        public string HashTag { get; set; } = "";
        /// <summary>
        /// 自动识别关键字, | 分隔
        /// </summary>
        public string KeyWords { get; set; } = "";
        /// <summary>
        /// 投稿时识别到此Tag是否自动添加遮罩, 手动添加Tag时不生效
        /// </summary>
        public bool AutoSpoiler { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateAt { get; set; } = DateTime.Now;
    }
}
