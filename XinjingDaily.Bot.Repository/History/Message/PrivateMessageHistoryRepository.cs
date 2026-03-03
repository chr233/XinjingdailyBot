using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.History.Message;
using XinjingDaily.Bot.IRepository.History.Message;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.History.Message;

/// <summary>
/// 私聊消息历史仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class PrivateMessageHistoryRepository : RepositoryInt<PrivateMessageHistory>, IPrivateMessageHistoryRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public PrivateMessageHistoryRepository(ISqlSugarClient db) : base(db)
    {
    }
}
