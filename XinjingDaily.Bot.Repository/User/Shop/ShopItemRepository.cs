using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.User.Shop;
using XinjingDaily.Bot.IRepository.User.Shop;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.User.Shop;

/// <summary>
/// 角色权限仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class ShopItemRepository : RepositoryInt<ShopItem>, IShopItemRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public ShopItemRepository(ISqlSugarClient db) : base(db)
    {
    }
}
