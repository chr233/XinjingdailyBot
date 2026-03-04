using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.History.Operate;
using XinjingDaily.Bot.IRepository.History.Operate;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.History.Operate;

/// <summary>
/// 封禁历史仓储实现
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="db"></param>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class BanHistoryRepository(ISqlSugarClient db) : RepositoryInt<BanHistory>(db), IBanHistoryRepository
{
}
