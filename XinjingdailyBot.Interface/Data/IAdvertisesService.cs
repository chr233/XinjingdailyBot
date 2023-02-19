using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    public interface IAdvertisesService : IBaseService<Advertises>
    {
        Task<Advertises?> GetPostableAdvertise();
    }
}
