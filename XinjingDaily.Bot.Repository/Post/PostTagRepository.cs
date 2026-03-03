using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Posts;
using XinjingDaily.Bot.IRepository.Post;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Post;

/// <summary>
/// 帖子标签仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class PostTagRepository : RepositoryInt<PostTag>, IPostTagRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public PostTagRepository(ISqlSugarClient db) : base(db)
    {
    }
}
