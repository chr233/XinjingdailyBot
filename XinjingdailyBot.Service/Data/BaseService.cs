using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Service.Data
{
    public class BaseService<T> : BaseRepository<T> where T : BaseModel, new()
    {
    }
}
