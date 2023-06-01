using SqlSugar;
using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Model.Models;

/// <summary>
/// 用户密钥表, 储存WebAPI的Token
/// </summary>
[SugarTable("user_token", TableDescription = "用户密钥表")]
[SugarIndex("index_token", nameof(APIToken), OrderByType.Asc, false)]
public sealed record UserTokens : BaseModel, ICreateAt, IExpiredAt
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public long UserID { get; set; }

    /// <summary>
    /// API Token
    /// </summary>
    public Guid APIToken { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateAt { get; set; } = DateTime.Now;
    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime ExpiredAt { get; set; } = DateTime.MaxValue;
}
