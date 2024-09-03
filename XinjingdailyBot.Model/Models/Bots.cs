using SqlSugar;
using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Model.Models;

/// <summary>
/// 新的稿件表
/// </summary>
[SugarTable("bot", TableDescription = "机器人设置")]

public sealed record Bots : BaseModel, IModifyAt, ICreateAt
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
    /// 设为主要机器人
    /// </summary>
    public bool Primary { get; set; }

    /// <summary>
    /// 机器人Token
    /// </summary>
    [SugarColumn(Length = 50)]
    public string BotToken { get; set; } = "";

    /// <summary>
    /// 机器人用户Id
    /// </summary>
    public long UserId { get; set; } = -1;

    /// <summary>
    /// 机器人用户名@
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    /// 机器人昵称
    /// </summary>
    public string? Nickname { get; set; } = "";

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;
    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;
}
