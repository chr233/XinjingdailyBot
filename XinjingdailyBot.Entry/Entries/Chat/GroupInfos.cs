using SqlSugar;

namespace XinjingdailyBot.Entry.Entries.Chats;

[SugarTable("group_info", TableDescription = "群聊信息")]
public sealed record GroupInfos
{
    [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
    public int Id { get; set; }

    public int TelegramChatId { get; set; }
    public string TelegramName { get; set; }
    public string ChatTitle { get; set; }



}
