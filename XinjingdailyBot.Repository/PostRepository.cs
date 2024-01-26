using SqlSugar;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository;

/// <summary>
/// 稿件仓储类
/// </summary>
[AppService(LifeTime.Transient)]
public class PostRepository(ISqlSugarClient context) : BaseRepository<NewPosts>(context)
{
}
