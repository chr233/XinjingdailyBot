using SqlSugar;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Entry.Entries.Users;

/// <summary>
/// 用户密钥表, 储存WebAPI的Token
/// </summary>
[SugarTable("user_token", TableDescription = "用户密钥表")]
[SugarIndex("i_usertoken_apitoken", nameof(ApiToken), OrderByType.Asc, false)]
public sealed record UserTokens : ICreateAt, IExpiredAt
{
    /// <summary>
    /// 用户表主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// UserInfo 主键
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// API Token
    /// </summary>
    public Guid ApiToken { get; set; }

    /// <inheritdoc cref=" ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;
    /// <inheritdoc cref=" IExpiredAt"/>
    public DateTime ExpiredAt { get; set; } = DateTime.MaxValue;
}
