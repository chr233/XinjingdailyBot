using SqlSugar;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository;

/// <summary>
/// 旧稿件表仓储类
/// </summary>
/// <param name="context"></param>
//[AppService(LifeTime.Transient)]
[Obsolete("废弃的仓储类")]
public class OldPostRepository(ISqlSugarClient context) : BaseRepository<OldPosts>(context)
{
}
