using Telegram.Bot.Types;
using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data;

/// <summary>
/// 广告服务
/// </summary>
public interface IAdvertiseService : IBaseService<Advertises>
{
    /// <summary>
    /// 创建广告
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task CreateAdvertise(Message message);

    /// <summary>
    /// 获取可用的广告消息
    /// </summary>
    /// <returns></returns>
    Task<Advertises?> GetPostableAdvertise();
    /// <summary>
    /// 更新广告统计
    /// </summary>
    /// <param name="ad"></param>
    /// <returns></returns>
    Task UpdateAdvertiseStatistics(Advertises ad);
}
