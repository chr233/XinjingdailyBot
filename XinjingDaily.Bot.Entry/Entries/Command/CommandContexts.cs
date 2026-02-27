using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Command;

/// <summary>
/// 命令上下文表
/// </summary>
[SugarTable("command_context", TableDescription = "命令上下文")]
[SugarIndex("i_command_context_userid_chatId", nameof(UserId), OrderByType.Asc, nameof(ChatId), OrderByType.Asc, true)]
public sealed record CommandContexts : ICreateAt, IModifyAt
{
    [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
    public int Id { get; set; }

    /// <summary>
    /// UserInfo主键
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Telegram ChatId
    /// </summary>
    public long ChatId { get; set; }

    /// <summary>
    /// 上下文JSON内容
    /// </summary>
    [SugarColumn(IsNullable = true, Length = 2000)]
    public string? Context { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime CreateAt { get; set; }
    public DateTime ModifyAt { get; set; }
}