using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.User.Shop;

/// <summary>
/// 用户代币表
/// </summary>
[SugarTable("user_coins", TableDescription = "用户代币")]
[SugarIndex("i_user_coins_userid", nameof(UserId), OrderByType.Asc, true)]
public sealed record UserCoins : ICreateAt, IModifyAt
{
    [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
    public int Id { get; set; }
    /// <summary>
    /// UserInfo 主键
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 代币数量
    /// </summary>
    public int CoinCount { get; set; } = 0;

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; }

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; }
}