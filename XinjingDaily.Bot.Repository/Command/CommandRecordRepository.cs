using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Command;
using XinjingDaily.Bot.IRepository.Command;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Command;

/// <summary>
/// 频道设置仓储实现
/// </summary>
/// <param name="db"></param>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class CommandRecordRepository(ISqlSugarClient db) : RepositoryInt<CommandRecord>(db), ICommandRecordRepository
{
}
