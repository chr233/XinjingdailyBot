using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.History.Message;
using XinjingDaily.Bot.IRepository.History.Message;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.History.Message;

/// <summary>
/// 私聊消息历史仓储实现
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="db"></param>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class PrivateMessageHistoryRepository(ISqlSugarClient db) : RepositoryInt<PrivateMessageHistory>(db), IPrivateMessageHistoryRepository
{
}
