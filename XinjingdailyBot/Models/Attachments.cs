using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;

namespace XinjingdailyBot.Models
{
    [SugarTable("attachment", TableDescription = "投稿附件")]
    [SugarIndex("index_postid", nameof(PostID), OrderByType.Asc)]
    internal sealed class Attachments
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public ulong Id { get; set; }
        /// <summary>
        /// 稿件ID
        /// </summary>
        public ulong PostID { get; set; }
        /// <summary>
        /// 文件ID
        /// </summary>
        public string FileID { get; set; } = "";
        /// <summary>
        /// 文件唯一ID
        /// </summary>
        public string FileUniqueID { get; set; } = "";
        public int Size { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
