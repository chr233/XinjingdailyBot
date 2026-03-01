using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Historys;
using XinjingDaily.Bot.IRepository.Operate;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Operate;

/// <summary>
/// 封禁历史仓储实现
/// </summary>
[RegisterScoped]
public class BanHistorysRepository : RepositoryInt<BanHistory>, IBanHistorysRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public BanHistorysRepository(ISqlSugarClient db) : base(db)
    {
    }
}
