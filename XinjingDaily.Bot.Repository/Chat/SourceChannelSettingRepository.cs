using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Channel;
using XinjingDaily.Bot.IRepository.Channel;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Channel;

/// <summary>
/// 机器人频道仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class SourceChannelSettingRepository(ISqlSugarClient db) : RepositoryInt<SourceChannelSetting>(db), ISourceChannelSettingRepository
{

}
