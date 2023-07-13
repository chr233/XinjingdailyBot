using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data;

/// <summary>
/// 广告服务
/// </summary>
public interface IAdvertiseService : IBaseService<Advertises>
{
    /// <summary>
    /// 获取可用的广告消息
    /// </summary>
    /// <returns></returns>
    Task<Advertises?> GetPostableAdvertise();
}
