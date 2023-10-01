using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data;

/// <summary>
/// 广告消息服务
/// </summary>
public interface IAdvertisePostService : IBaseService<AdvertisePosts>
{
    /// <summary>
    /// 删除旧的广告消息
    /// </summary>
    /// <param name="advertises"></param>
    /// <param name="chatId"></param>
    /// <returns></returns>
    Task DeleteOldAdPosts(Advertises advertises, long chatId);
    /// <summary>
    /// 删除旧的广告消息
    /// </summary>
    /// <param name="advertises"></param>
    /// <returns></returns>
    Task DeleteOldAdPosts(Advertises advertises);

    /// <summary>
    /// 是否为第一次发布
    /// </summary>
    /// <param name="advertises"></param>
    /// <returns></returns>
    Task<bool> IsFirstAdPost(Advertises advertises);
}
