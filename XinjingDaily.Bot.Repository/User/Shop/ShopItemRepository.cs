using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.User.Shop;
using XinjingDaily.Bot.IRepository.User.Shop;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.User.Shop;

/// <summary>
/// 角色权限仓储实现
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="db"></param>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class ShopItemRepository(ISqlSugarClient db) : RepositoryInt<ShopItem>(db), IShopItemRepository
{
}
