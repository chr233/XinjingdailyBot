using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.History.Message;
using XinjingDaily.Bot.IRepository.History.Message;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.History.Message;

/// <summary>
/// 群消息历史仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class GroupMessageHistoryRepository : RepositoryInt<GroupMessageHistory>, IGroupMessageHistoryRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public GroupMessageHistoryRepository(ISqlSugarClient db) : base(db)
    {
    }
}
