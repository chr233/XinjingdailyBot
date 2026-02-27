using SqlSugar;

namespace XinjingDaily.Bot.Entry.Entries.Chats;

[SugarTable("group_info", TableDescription = "群聊信息")]
public sealed record ChatInfos
{
    [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
    public int Id { get; set; }

    public long TelegramId { get; set; }

    [SugarColumn(IsNullable = true)]
    public string? TelegramName { get; set; }

    [SugarColumn(IsNullable = true)]
    public string? ChatTitle { get; set; }

    public int MemberCount { get; set; }

}
