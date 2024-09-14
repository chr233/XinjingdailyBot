using SqlSugar;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Model.Base;

namespace XinjingdailyBot.Model.Models;

/// <summary>
/// 用户表, 储存所有用户的基本信息, 权限设定, 以及投稿信息统计
/// </summary>
[SugarTable("user_acl", TableDescription = "用户权限表")]
[SugarIndex("ua_userid", nameof(UserID), OrderByType.Asc, true)]
//[SugarIndex("index_username", nameof(UserName), OrderByType.Asc)]
public sealed record UserACLs : BaseModel
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
