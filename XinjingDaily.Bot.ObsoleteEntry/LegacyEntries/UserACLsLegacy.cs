using SqlSugar;
using XinjingDaily.Bot.Infrastructure.Enums;

namespace XinjingDaily.Bot.Model.Legacy;

/// <summary>
/// 用户表, 储存所有用户的基本信息, 权限设定, 以及投稿信息统计
/// </summary>
[Obsolete]
[SugarTable("user_acl", TableDescription = "用户权限表")]
[SugarIndex("ua_userid", nameof(UserID), OrderByType.Asc, true)]
//[SugarIndex("index_username", nameof(UserName), OrderByType.Asc)]
public sealed record UserACLsLegacy
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserID { get; set; }

    public long ChannelId { get; set; }

    public EUserRights Right { get; set; }

}
