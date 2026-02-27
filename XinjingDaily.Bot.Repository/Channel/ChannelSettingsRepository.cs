using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Channel;
using XinjingDaily.Bot.IRepository.Channel;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Channel;

/// <summary>
/// 频道设置仓储实现
/// </summary>
[RegisterScoped]
public class ChannelSettingsRepository : RepositoryInt<SourceChannelSettings>, IChannelSettingsRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public ChannelSettingsRepository(ISqlSugarClient db) : base(db)
    {
    }
}
