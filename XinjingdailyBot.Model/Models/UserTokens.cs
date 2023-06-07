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
    /// 用户表主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int UID { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserID { get; set; }

    /// <summary>
    /// API Token
    /// </summary>
    public Guid APIToken { get; set; }

    /// <summary>
    /// 用户数据
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(UID))]//一对一 SchoolId是StudentA类里面的
    public Users? User { get; set; } //不能赋值只能是null

    /// <inheritdoc cref=" ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;
    /// <inheritdoc cref=" IExpiredAt"/>
    public DateTime ExpiredAt { get; set; } = DateTime.MaxValue;
}
