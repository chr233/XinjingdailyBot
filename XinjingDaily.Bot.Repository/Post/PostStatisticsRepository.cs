using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Posts;
using XinjingDaily.Bot.IRepository.Post;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Post;

/// <summary>
/// 帖子统计仓储实现
/// </summary>
[RegisterScoped]
public class PostStatisticsRepository : RepositoryInt<PostStatistics>, IPostStatisticsRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public PostStatisticsRepository(ISqlSugarClient db) : base(db)
    {
    }
}
