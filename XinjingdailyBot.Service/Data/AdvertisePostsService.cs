using Microsoft.Extensions.Logging;
using SqlSugar;
using Telegram.Bot;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data;

/// <inheritdoc cref="IAdvertisePostService"/>
[AppService(typeof(IAdvertisePostService), LifeTime.Transient)]
public sealed class AdvertisePostsService(
    ILogger<AdvertisePostsService> _logger,
    ITelegramBotClient _botClient,
    ISqlSugarClient context) : BaseService<AdvertisePosts>(context), IAdvertisePostService
{
    /// <inheritdoc/>
    public async Task DeleteOldAdPosts(Advertises advertises)
    {
        var oldPosts = await Queryable()
            .Where(x => x.AdId == advertises.Id && !x.Deleted)
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

    /// <inheritdoc/>
    public async Task UnPinOldAdPosts(Advertises advertises)
    {
        var oldPosts = await Queryable()
            .Where(x => x.AdId == advertises.Id && x.Pined)
            .ToListAsync();

        foreach (var oldPost in oldPosts)
        {
            try
            {
                await _botClient.UnpinChatMessageAsync(oldPost.ChatID, (int)oldPost.MessageID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消置顶消息失败");
                await Task.Delay(500);
            }
            finally
            {
                oldPost.Pined = false;
                oldPost.ModifyAt = DateTime.Now;
                await Updateable(oldPost)
                    .UpdateColumns(static x => new { x.Pined, x.ModifyAt })
                    .ExecuteCommandAsync();
            }
        }
    }

    /// <inheritdoc/>
    public async Task AddAdPost(Advertises ad, long chatId, int msgId)
    {
        var adpost = new AdvertisePosts {
            AdId = ad.Id,
            ChatID = chatId,
            MessageID = msgId,
            Pined = ad.PinMessage,
            CreateAt = DateTime.Now,
            ModifyAt = DateTime.Now,
        };

        await Insertable(adpost).ExecuteCommandAsync();
    }
}
