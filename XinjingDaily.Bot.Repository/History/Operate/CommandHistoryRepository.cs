using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.History.Operate;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.IRepository.History.Operate;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.History.Operate;

/// <summary>
/// 频道设置仓储实现
/// </summary>
/// <param name="db"></param>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class CommandHistoryRepository(ISqlSugarClient db) : RepositoryInt<CommandHistory>(db), ICommandHistoryRepository
{
    public async Task InsertCommandRecord(UserInfo userInfo, long chatId, string command)
    {

    }
}
