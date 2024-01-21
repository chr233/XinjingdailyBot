using SqlSugar;
using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Service.Data.Base;

/// <summary>
/// 基础仓储服务
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseService<T>(ISqlSugarClient context) : BaseRepository<T>(context) where T : BaseModel, new()
{
}
