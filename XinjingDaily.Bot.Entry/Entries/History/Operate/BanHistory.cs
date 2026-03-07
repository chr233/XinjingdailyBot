using SqlSugar;
using XinjingDaily.Bot.Infrastructure.Enums;

namespace XinjingDaily.Bot.Entry.Entries.History.Operate;

/// <summary>
/// 用户封禁记录
/// </summary>
[SugarTable("ban_history", TableDescription = "用户封禁记录户表")]
[SugarIndex("index_userid", nameof(UserId), OrderByType.Asc)]
[SugarIndex("index_operatorid", nameof(OperatorUserId), OrderByType.Asc)]
public sealed record BanHistory
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    /// <summary>
    /// UserInfo主键
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// UserInfo主键, 操作者Id
    /// </summary>
    public int OperatorUserId { get; set; }

    /// <summary>
    /// 是否封禁 true: 封禁, false: 解封
    /// </summary>
    public EBanType Type { get; set; } = EBanType.UnBan;

    /// <summary>
    /// 封禁时间
    /// </summary>
    public DateTime BanTime { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// 封禁理由
    /// </summary>
    public string Reason { get; set; } = "";
}
