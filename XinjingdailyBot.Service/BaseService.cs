using XinjingdailyBot.Model.Enums.Base;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Service
{
    public class BaseService<T> : BaseRepository<T> where T : BaseModel, new()
    {
        //public IBaseRepository<T> baseRepository;

        //public BaseService(IBaseRepository<T> repository)
        //{
        //    this.baseRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        //}
    }
}