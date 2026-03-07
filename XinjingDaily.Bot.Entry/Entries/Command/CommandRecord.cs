using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Command;

[SugarTable("command_record", TableDescription = "命令调用记录")]
public sealed record CommandRecord : ICreateAt
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// UserInfo主键
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// CommandContext主键
    /// </summary>
    public int ContextId { get; set; }

    public long TgChatId { get; set; }

    public int TgMessageId { get; set; }

    [SugarColumn(IsNullable = true)]
    public string? Command { get; set; }

    [SugarColumn(IsNullable = true)]
    public string? Exception { get; set; }

    public bool IsQueryCommand { get; set; }
    public bool IsSuccess { get; set; }

    public bool IsHandled { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
}
