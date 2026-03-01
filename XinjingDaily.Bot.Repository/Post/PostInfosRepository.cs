using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Posts;
using XinjingDaily.Bot.IRepository.Post;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Post;

/// <summary>
/// 帖子信息仓储实现
/// </summary>
[RegisterScoped]
public class PostInfosRepository : RepositoryInt<PostInfo>, IPostInfosRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public PostInfosRepository(ISqlSugarClient db) : base(db)
    {
    }
}
