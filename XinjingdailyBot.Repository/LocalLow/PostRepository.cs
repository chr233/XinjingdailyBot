using SqlSugar;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository.Repositorys;

/// <summary>
/// 稿件仓储类
/// </summary>
[AppService(LifeTime.Transient)]
public class PostRepository(ISqlSugarClient _context) : BaseRepository<Posts>(_context)
{
}
