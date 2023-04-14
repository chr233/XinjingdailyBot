using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    public interface IAdvertisesService : IBaseService<Advertises>
    {
        /// <summary>
        /// 获取可用的广告消息
        /// </summary>
        /// <returns></returns>
        Task<Advertises?> GetPostableAdvertise();
    }
}
