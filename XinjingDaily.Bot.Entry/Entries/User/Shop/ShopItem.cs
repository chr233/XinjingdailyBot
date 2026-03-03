using SqlSugar;
using XinjingDaily.Bot.Entry.Columns;

namespace XinjingDaily.Bot.Entry.Entries.User.Shop;

/// <summary>
/// 商店物品表（徽章兑换）
/// </summary>
[SugarTable("shop_items", TableDescription = "商店物品表")]
[SugarIndex("i_shop_items_badge_id", nameof(BadgeId), OrderByType.Asc, true)]
public sealed record ShopItem : ICreateAt, IModifyAt
{
    [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
    public int Id { get; set; }

    /// <summary>
    /// 物品名称
    /// </summary>
    [SugarColumn(Length = 128)]
    public string Name { get; set; } = "";

    /// <summary>
    /// 关联徽章ID
    /// </summary>
    public int BadgeId { get; set; }

    /// <summary>
    /// 兑换所需代币
    /// </summary>
    public int RequiredCoins { get; set; }

    /// <summary>
    /// 是否上架
    /// </summary>
    public bool IsOnShelf { get; set; } = true;

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; }

    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; }
}