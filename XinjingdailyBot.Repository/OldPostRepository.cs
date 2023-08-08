using SqlSugar;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository;

/// <summary>
/// 旧稿件表仓储类
/// </summary>
[AppService(LifeTime.Transient)]
[Obsolete("废弃的仓储类")]
public class OldPostRepository : BaseRepository<OldPosts>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public OldPostRepository(ISqlSugarClient context) : base(context)
    {
    }
}
