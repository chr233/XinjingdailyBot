using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Ads;
using XinjingDaily.Bot.IRepository.Ads;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Ads;

/// <summary>
/// 广告历史仓储实现
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="db"></param>
[RegisterScoped]
public class AdvertiseHistorysRepository(ISqlSugarClient db) : RepositoryInt<AdvertiseHistorys>(db), IAdvertiseHistorysRepository
{
}
