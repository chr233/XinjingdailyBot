using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Chats;

[SugarTable("group_info", TableDescription = "群聊信息")]
public sealed record GroupInfo :ICreateAt, IModifyAt
{
    [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
    public int Id { get; set; }

    public long TelegramId { get; set; }

    [SugarColumn(IsNullable = true)]
    public string? TelegramName { get; set; }

    [SugarColumn(IsNullable = true)]
    public string? ChatTitle { get; set; }

    public int MemberCount { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; }

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; }
}
