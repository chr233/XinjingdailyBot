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
public class CommandContextRepository(ISqlSugarClient db) : RepositoryInt<CommandContext>(db), ICommandContextRepository
{
}
