using SqlSugar;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Model.Enums.Base;

namespace XinjingdailyBot.Model.Models
{
    [SugarTable("attachment", TableDescription = "投稿附件")]
    [SugarIndex("index_post_id", nameof(PostID), OrderByType.Asc)]
    public sealed record Attachments : BaseModel
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }
        /// <summary>
        /// 稿件ID
        /// </summary>
        public long PostID { get; set; }
        /// <summary>
        /// 文件ID
        /// </summary>
        public string FileID { get; set; } = "";
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; } = "";
        /// <summary>
        /// 文件唯一ID
        /// </summary>
        public string FileUniqueID { get; set; } = "";
        /// <summary>
        /// 文件类型
        /// </summary>
        public string MimeType { get; set; } = "";
        /// <summary>
        /// 文件尺寸
        /// </summary>
        public long Size { get; set; }
        /// <summary>
        /// 图像高度
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// 图像宽度
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType Type { get; set; } = MessageType.Unknown;
    }
}
