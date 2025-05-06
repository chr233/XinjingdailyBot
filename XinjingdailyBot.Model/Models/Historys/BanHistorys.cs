using SqlSugar;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models.Historys;

/// <summary>
/// 用户封禁记录
/// </summary>
[Obsolete]
[SugarTable("xjb_ban_history", TableDescription = "用户w封禁记录户表")]
[SugarIndex("index_userid", nameof(UserId), OrderByType.Asc)]
[SugarIndex("index_operatorid", nameof(OperatorId), OrderByType.Asc)]
[SugarIndex("index_userid_type", nameof(UserId), OrderByType.Asc, nameof(Type), OrderByType.Asc)]
[SugarIndex("index_userid_bantime", nameof(UserId), OrderByType.Asc, nameof(BanTime), OrderByType.Desc)]
public sealed record BanHistorys : BaseModel
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }
    /// <summary>
    /// 执行封禁操作的管理员ID
    /// </summary>
    public long OperatorId { get; set; }
    /// <summary>
    /// 是否封禁 true: 封禁, false: 解封
    /// </summary>
    public EBanType Type { get; set; } = EBanType.UnBan;
    /// <summary>
    /// 封禁时间
    /// </summary>
    public DateTime BanTime { get; set; } = DateTime.Now;
    /// <summary>
    /// 封禁理由
    /// </summary>
    public string Reason { get; set; } = "";
}
