using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Posts;
using XinjingDaily.Bot.IRepository.Post;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Post;

/// <summary>
/// 媒体组信息仓储实现
/// </summary>
[RegisterScoped]
public class MediaGroupInfosRepository : RepositoryInt<MediaGroupInfo>, IMediaGroupInfosRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public MediaGroupInfosRepository(ISqlSugarClient db) : base(db)
    {
    }
}
