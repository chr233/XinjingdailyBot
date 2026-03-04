using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Channel;
using XinjingDaily.Bot.IRepository.Channel;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Channel;

/// <summary>
/// 频道设置仓储实现
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="db"></param>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class ChannelSettingRepository(ISqlSugarClient db) : RepositoryInt<SourceChannelSetting>(db), ISourceChannelSettingRepository
{
}
