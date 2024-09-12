using SqlSugar;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models;

/// <summary>
/// 新的稿件表
/// </summary>
[SugarTable("bot_chat", TableDescription = "机器人会话")]
[SugarIndex("bc_bu", nameof(BotId), OrderByType.Asc, nameof(UserId), OrderByType.Asc, true)]
[SugarIndex("bc_cu", nameof(BotId), OrderByType.Asc, nameof(ChatId), OrderByType.Asc, true)]

public sealed record BotChats : BaseModel
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    public int BotId { get; set; }
    public long UserId { get; set; }
    public long ChatId { get; set; }
}
