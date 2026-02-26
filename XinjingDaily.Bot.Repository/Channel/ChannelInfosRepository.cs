using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Posts;
using XinjingDaily.Bot.IRepository.Channel;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Channel;

/// <summary>
/// 频道信息仓储实现
/// </summary>
public class ChannelInfosRepository : RepositoryInt<ChannelInfos>, IChannelInfoRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public ChannelInfosRepository(ISqlSugarClient db) : base(db)
    {
    }
}
