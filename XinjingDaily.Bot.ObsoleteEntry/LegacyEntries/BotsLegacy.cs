using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.ObsoleteEntry.LegacyEntries;

/// <summary>
/// 新的稿件表
/// </summary>
[Obsolete]
[SugarTable("bot", TableDescription = "机器人设置")]
[SugarIndex("bot_bt", nameof(BotToken), OrderByType.Asc, true)]

public sealed record BotsLegacy : IModifyAt, ICreateAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 启用机器人
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 机器人Token
    /// </summary>
    [SugarColumn(Length = 50)]
    public string BotToken { get; set; } = null!;

    /// <summary>
    /// 机器人权重, 权重越大被使用的概率越高
    /// </summary>
    public byte Weight { get; set; }

    /// <summary>
    /// 机器人用户Id
    /// </summary>
    public long UserId { get; set; } = -1;

    /// <summary>
    /// 机器人用户名@
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Username { get; set; }

    /// <summary>
    /// 机器人昵称
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Firstname { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.MinValue;
}
