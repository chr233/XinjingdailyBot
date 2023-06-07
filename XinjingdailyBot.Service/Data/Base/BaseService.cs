using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Service.Data.Base;

/// <summary>
/// 基础仓储服务
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseService<T> : BaseRepository<T> where T : BaseModel, new()
{
}
