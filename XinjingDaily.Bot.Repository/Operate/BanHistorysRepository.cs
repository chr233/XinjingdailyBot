using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.History.Operate;
using XinjingDaily.Bot.IRepository.Operate;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Operate;

/// <summary>
/// 封禁历史仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class BanHistorysRepository : RepositoryInt<BanHistory>, IBanHistoryRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public BanHistorysRepository(ISqlSugarClient db) : base(db)
    {
    }
}
