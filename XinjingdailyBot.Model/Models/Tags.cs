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
        /// 位
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int Seg { get; set; }
        /// <summary>
        /// 标签名
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// CallbackData
        /// </summary>
        public string Payload { get; set; } = "";
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
        /// 警告文本, 带有此Tag的投稿会在发布时提前发送警告
        /// </summary>
        public string WarnText { get; set; } = "";
    }
}
