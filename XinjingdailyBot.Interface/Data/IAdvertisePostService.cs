using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data;

/// <summary>
/// 广告消息服务
/// </summary>
public interface IAdvertisePostService : IBaseService<AdvertisePosts>
{
    Task AddAdPost(Advertises ad, long chatId, int msgId);

    /// <summary>
    /// 删除旧的广告消息
    /// </summary>
    /// <param name="advertises"></param>
    /// <returns></returns>
    Task DeleteOldAdPosts(Advertises advertises);
    /// <summary>
    /// 取消置顶广告消息
    /// </summary>
    /// <param name="advertises"></param>
    /// <returns></returns>
    Task UnPinOldAdPosts(Advertises advertises);
}
