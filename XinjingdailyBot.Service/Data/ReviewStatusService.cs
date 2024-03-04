using Microsoft.Extensions.Logging;
using SqlSugar;
using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data;

/// <inheritdoc cref="IReviewStatusService"/>
[AppService(typeof(IReviewStatusService), LifeTime.Transient)]
public sealed class ReviewStatusService(
    ILogger<ReviewStatusService> _logger,
    ITelegramBotClient _botClient,
    ISqlSugarClient context) : BaseService<ReviewStatus>(context), IReviewStatusService
{
    /// <inheritdoc/>
    public async Task<ReviewStatus?> GetOldReviewStatu()
    {
        var oldPost = await Queryable().FirstAsync(static x => !x.Deleted).ConfigureAwait(false);
        return oldPost;
    }

    /// <inheritdoc/>
    public async Task DeleteOldReviewStatus()
    {
        var oldPosts = await Queryable()
            .Where(static x => !x.Deleted)
            .ToListAsync().ConfigureAwait(false);

        foreach (var oldPost in oldPosts)
        {
            try
            {
                await _botClient.DeleteMessageAsync(oldPost.ChatID, (int)oldPost.MessageID).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除消息失败");
                await Task.Delay(500).ConfigureAwait(false);
            }
            finally
            {
                oldPost.Deleted = true;
                oldPost.ModifyAt = DateTime.Now;
                await Updateable(oldPost)
                    .UpdateColumns(static x => new { x.Deleted, x.ModifyAt })
                    .ExecuteCommandAsync().ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc/>
    public Task CreateNewReviewStatus(Message message)
    {
        var reviewStatus = new ReviewStatus {
            ChatID = message.Chat.Id,
            MessageID = message.MessageId,
            CreateAt = DateTime.Now,
            ModifyAt = DateTime.Now,
            Deleted = false,
        };

        return Insertable(reviewStatus).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public Task DeleteReviewStatus(ReviewStatus reviewStatus)
    {
        reviewStatus.Deleted = true;
        reviewStatus.ModifyAt = DateTime.Now;
        return Updateable(reviewStatus)
            .UpdateColumns(static x => new { x.Deleted, x.ModifyAt })
            .ExecuteCommandAsync();
    }
}
