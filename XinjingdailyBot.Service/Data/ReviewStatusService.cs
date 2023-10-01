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
internal sealed class ReviewStatusService : BaseService<ReviewStatus>, IReviewStatusService
{
    private readonly ILogger<ReviewStatusService> _logger;
    private readonly ITelegramBotClient _botClient;

    public ReviewStatusService(
        ILogger<ReviewStatusService> logger,
        ITelegramBotClient botClient,
        ISqlSugarClient context) : base(context)
    {
        _logger = logger;
        _botClient = botClient;
    }

    public async Task<ReviewStatus?> GetOldReviewStatu()
    {
        var oldPost = await Queryable().FirstAsync(static x => !x.Deleted);
        return oldPost;
    }

    public async Task DeleteOldReviewStatus()
    {
        var oldPosts = await Queryable()
            .Where(static x => !x.Deleted)
            .ToListAsync();

        foreach (var oldPost in oldPosts)
        {
            try
            {
                await _botClient.DeleteMessageAsync(oldPost.ChatID, (int)oldPost.MessageID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除消息失败");
                await Task.Delay(500);
            }
            finally
            {
                oldPost.Deleted = true;
                oldPost.ModifyAt = DateTime.Now;
                await Updateable(oldPost)
                    .UpdateColumns(static x => new { x.Deleted, x.ModifyAt })
                    .ExecuteCommandAsync();
            }
        }
    }

    public async Task CreateNewReviewStatus(Message message)
    {
        var reviewStatus = new ReviewStatus {
            ChatID = message.Chat.Id,
            MessageID = message.MessageId,
            CreateAt = DateTime.Now,
            ModifyAt = DateTime.Now,
            Deleted = false,
        };

        await Insertable(reviewStatus).ExecuteCommandAsync();
    }
}
