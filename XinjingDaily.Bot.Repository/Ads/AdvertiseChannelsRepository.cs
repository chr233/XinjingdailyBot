using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Ads;
using XinjingDaily.Bot.IRepository.Ads;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Ads;

/// <summary>
/// 广告频道仓储实现
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="db"></param>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class AdvertiseChannelsRepository(ISqlSugarClient db) : RepositoryInt<AdvertiseChat>(db), IAdvertiseChannelRepository
{
}
