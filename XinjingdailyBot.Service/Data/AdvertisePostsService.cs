using Microsoft.Extensions.Logging;
using Telegram.Bot;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data;

/// <inheritdoc cref="IAdvertisePostService"/>
[AppService(typeof(IAdvertisePostService), LifeTime.Transient)]
internal sealed class AdvertisePostsService : BaseService<AdvertisePosts>, IAdvertisePostService
{
    private readonly ILogger<AdvertisePostsService> _logger;
    private readonly ITelegramBotClient _botClient;

    public AdvertisePostsService(
        ILogger<AdvertisePostsService> logger,
        ITelegramBotClient botClient)
    {
        _logger = logger;
        _botClient = botClient;
    }

    public async Task DeleteOldAdPosts(Advertises advertises, bool excludePin)
    {
        var oldPosts = await Queryable()
            .Where(x => x.AdId == advertises.Id && !x.Deleted)
            .WhereIF(excludePin, static x => !x.Pined)
            .ToListAsync();

        foreach (var oldPost in oldPosts)
        {
            try
            {
                oldPost.Deleted = true;
                oldPost.ModifyAt = DateTime.Now;
                await Updateable(oldPost).ExecuteCommandAsync();

                await _botClient.DeleteMessageAsync(oldPost.ChatID, (int)oldPost.MessageID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除消息失败");
                await Task.Delay(500);
            }
            finally
            {
                await Updateable(oldPost).ExecuteCommandAsync();
            }
        }
    }

    public async Task DeleteOldAdPosts(Advertises advertises, long chatId, bool excludePin)
    {
        var oldPosts = await Queryable()
            .Where(x => x.AdId == advertises.Id && x.ChatID == chatId && !x.Deleted)
            .WhereIF(excludePin, static x => !x.Pined)
            .ToListAsync();

        foreach (var oldPost in oldPosts)
        {
            try
            {
                oldPost.Deleted = true;
                oldPost.ModifyAt = DateTime.Now;
                await Updateable(oldPost).ExecuteCommandAsync();

                await _botClient.DeleteMessageAsync(oldPost.ChatID, (int)oldPost.MessageID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除消息失败");
                await Task.Delay(500);
            }
            finally
            {
                await Updateable(oldPost).ExecuteCommandAsync();
            }
        }
    }

    public async Task<bool> IsFirstAdPost(Advertises advertises)
    {
        var post = await Queryable().Where(x => x.AdId == advertises.Id).FirstAsync();
        return post == null;
    }
}
