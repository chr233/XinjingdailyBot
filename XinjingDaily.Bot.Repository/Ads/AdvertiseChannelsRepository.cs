using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Ads;
using XinjingDaily.Bot.IRepository.Ads;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Ads;

/// <summary>
/// 广告频道仓储实现
/// </summary>
[RegisterScoped]
public class AdvertiseChannelsRepository : RepositoryInt<AdvertiseChannels>, IAdvertiseChannelsRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public AdvertiseChannelsRepository(ISqlSugarClient db) : base(db)
    {
    }
}
