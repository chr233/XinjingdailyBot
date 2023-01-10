using SqlSugar;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models
{
    [SugarTable("answer", TableDescription = "关键词回复规则")]
    public sealed record Answers : BaseModel
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public string Keyword { get; set; } = "";
        public string Answer { get; set; } = "";
    }
}
