using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.History.Operate;
using XinjingDaily.Bot.IRepository.History.Operate;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.History.Operate;

/// <summary>
/// 封禁历史仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class BanHistoryRepository : RepositoryInt<BanHistory>, IBanHistoryRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public BanHistoryRepository(ISqlSugarClient db) : base(db)
    {
    }
}
