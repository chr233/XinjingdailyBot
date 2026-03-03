using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Ads;
using XinjingDaily.Bot.IRepository.Ads;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Ads;

/// <summary>
/// 广告仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class AdvertiseRepository : RepositoryInt<Advertise>, IAdvertiseRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public AdvertiseRepository(ISqlSugarClient db) : base(db)
    {
    }
}
