using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data;
public interface IReviewStatusService
{
    Task CreateNewReviewStatus(Message message);
    Task DeleteOldReviewStatus();
    Task<ReviewStatus?> GetOldReviewStatu();
}