using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Ads;
using XinjingDaily.Bot.IRepository.Ads;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Ads;

/// <summary>
/// 广告统计仓储实现
/// </summary>
[RegisterScoped]
public class AdvertiseStatisticsRepository : RepositoryInt<AdvertiseStatistic>, IAdvertiseStatisticsRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public AdvertiseStatisticsRepository(ISqlSugarClient db) : base(db)
    {
    }
}
