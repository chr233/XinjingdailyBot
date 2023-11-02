using Telegram.Bot.Types;
using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data;
/// <summary>
/// 审核状态服务
/// </summary>
public interface IReviewStatusService : IBaseService<ReviewStatus>
{
    /// <summary>
    /// 创建新记录
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task CreateNewReviewStatus(Message message);
    /// <summary>
    /// 删除旧记录
    /// </summary>
    /// <returns></returns>
    Task DeleteOldReviewStatus();
    /// <summary>
    /// 删除记录
    /// </summary>
    /// <param name="reviewStatus"></param>
    /// <returns></returns>
    Task DeleteReviewStatus(ReviewStatus reviewStatus);
    /// <summary>
    /// 获取旧的审核状态
    /// </summary>
    /// <returns></returns>
    Task<ReviewStatus?> GetOldReviewStatu();
}