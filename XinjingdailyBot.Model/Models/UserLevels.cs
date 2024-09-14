using SqlSugar;
using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Model.Models;

/// <summary>
/// 用户表, 储存所有用户的基本信息, 权限设定, 以及投稿信息统计
/// </summary>
[SugarTable("user_level", TableDescription = "用户表")]
[SugarIndex("index_userid", nameof(UserID), OrderByType.Asc, true)]
public sealed record UserLevels : BaseModel, IModifyAt, ICreateAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int Id { get; set; }
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserID { get; set; }

    /// <summary>
    /// 通过的稿件数量
    /// </summary>
    public int AcceptCount { get; set; }
    /// <summary>
    /// 被拒绝的稿件数量
    /// </summary>
    public int RejectCount { get; set; }
    /// <summary>
    /// 过期未被审核的稿件数量(统计时总投稿需要减去此字段)
    /// </summary>
    public int ExpiredPostCount { get; set; }

    /// <summary>
    /// 投稿数量
    /// </summary>
    public int PostCount { get; set; }
    /// <summary>
    /// 审核数量
    /// </summary>
    public int ReviewCount { get; set; }
    /// <summary>
    /// 经验
    /// </summary>
    public ulong Experience { get; set; }
    /// <summary>
    /// 用户等级
    /// </summary>
    public int Level { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;
    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;
}
