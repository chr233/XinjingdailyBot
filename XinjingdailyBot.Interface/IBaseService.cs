using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Interface
{
    /// <summary>
    /// 基础服务定义
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBaseService<T> : IBaseRepository<T> where T : BaseModel, new()
    {
    }
}
