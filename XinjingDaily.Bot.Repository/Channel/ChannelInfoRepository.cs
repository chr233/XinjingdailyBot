using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Chat;
using XinjingDaily.Bot.IRepository.Channel;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Channel;

/// <summary>
/// 频道信息仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class ChannelInfoRepository : RepositoryInt<ChatInfo>, IChatInfoRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public ChannelInfoRepository(ISqlSugarClient db) : base(db)
    {
    }
}
