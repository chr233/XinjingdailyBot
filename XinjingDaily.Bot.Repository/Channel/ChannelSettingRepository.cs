using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Channel;
using XinjingDaily.Bot.IRepository.Channel;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Channel;

/// <summary>
/// 频道设置仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class ChannelSettingRepository : RepositoryInt<SourceChannelSetting>, ISourceChannelSettingRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public ChannelSettingRepository(ISqlSugarClient db) : base(db)
    {
    }
}
