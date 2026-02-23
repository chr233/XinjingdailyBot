using SqlSugar;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Entry.Entries.Chats;

[SugarTable("group_chat_history", TableDescription = "群聊消息记录")]
public sealed record GroupMessageHistory : ICreateAt, IModifyAt
{
    [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
    public int Id { get; set; }


    public int ChatId { get; set; }

    public int FromUserId { get; set; }
    public int ReplyToUserId { get; set; }



    public string? Content { get; set; }

    public MessageType Type { get; set; }

    public DateTime CreateAt { get; set; }
    public DateTime ModifyAt { get; set; }
}
