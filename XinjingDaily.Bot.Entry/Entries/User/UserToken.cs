using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.Users;

/// <summary>
/// 用户密钥表, 储存WebAPI的Token
/// </summary>
[SugarTable("user_token", TableDescription = "用户密钥表")]
[SugarIndex("i_user_token_api_token", nameof(ApiToken), OrderByType.Asc, false)]
[SugarIndex("i_user_token_user_id", nameof(UserId), OrderByType.Asc, true)]
public sealed record UserToken : ICreateAt, IExpiredAt
{
    /// <summary>
    /// 用户表主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// UserInfo 主键
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// API Token
    /// </summary>
    public Guid ApiToken { get; set; }

    /// <inheritdoc cref=" ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;
    /// <inheritdoc cref=" IExpiredAt"/>
    public DateTime ExpiredAt { get; set; } = DateTime.MaxValue;
}
