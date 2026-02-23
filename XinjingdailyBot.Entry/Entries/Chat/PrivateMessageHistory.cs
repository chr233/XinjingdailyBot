using SqlSugar;
using Telegram.Bot.Types.Enums;

namespace XinjingdailyBot.Entry.Entries.Chats;

[SugarTable("post_attachment", TableDescription = "投稿附件")]
public sealed record PrivateMessageHistory
{
    [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
    public int Id { get; set; }

    public MessageType Type { get; set; }


}
