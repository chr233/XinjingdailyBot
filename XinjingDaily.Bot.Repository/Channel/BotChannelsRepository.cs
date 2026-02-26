using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Channel;
using XinjingDaily.Bot.IRepository.Channel;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Channel;

/// <summary>
/// 机器人频道仓储实现
/// </summary>
[RegisterScoped]
public class BotChannelsRepository : RepositoryInt<BotChannels>, IBotChannelsRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public BotChannelsRepository(ISqlSugarClient db) : base(db)
    {
    }
}
