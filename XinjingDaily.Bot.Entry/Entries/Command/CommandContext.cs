using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Command;

[SugarTable("command_context", TableDescription = "命令上下文")]
[SugarIndex("i_command_context_chat_id_user_id", nameof(ChatId), OrderByType.Asc, nameof(UserId), OrderByType.Asc, true)]
public sealed record CommandContext : ICreateAt, IModifyAt
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// ChatInfo主键, 为-1表示私聊
    /// </summary>
    public int ChatId { get; set; }

    /// <summary>
    /// UserInfo主键, 为-1表示群聊共享上下文
    /// </summary>
    public int UserId { get; set; }

    [SugarColumn(IsNullable = true)]
    public string? Command { get; set; }

    [SugarColumn(IsNullable = true)]
    public string? Payload { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.MinValue;
}
