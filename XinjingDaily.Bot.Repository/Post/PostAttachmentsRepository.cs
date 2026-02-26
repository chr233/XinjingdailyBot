using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Posts;
using XinjingDaily.Bot.IRepository.Post;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Post;

/// <summary>
/// 帖子附件仓储实现
/// </summary>
[RegisterScoped]
public class PostAttachmentsRepository : RepositoryInt<PostAttachments>, IPostAttachmentsRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public PostAttachmentsRepository(ISqlSugarClient db) : base(db)
    {
    }
}
