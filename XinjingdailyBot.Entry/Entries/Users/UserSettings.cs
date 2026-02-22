using SqlSugar;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Entry.Entries.Users;

/// <summary>
/// 用户设置表
/// </summary>
[SugarTable("xjb_user_setting", TableDescription = "用户设置表")]
[SugarIndex("i_usersetting_userid", nameof(UserId), OrderByType.Asc, true)]
public sealed record UserSettings : IModifyAt, ICreateAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// UserInfo 主键
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 默认开启匿名模式
    /// </summary>
    public bool PreferAnonymous { get; set; }

    /// <summary>
    /// 是否开启通知
    /// </summary>
    public bool AllowNotification { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;
    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;
}
