using SqlSugar;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data;

/// <inheritdoc cref="IAdvertiseService"/>
[AppService(typeof(IAdvertiseService), LifeTime.Transient)]
public sealed class AdvertiseService(
    IAdvertisePostService _advertisePostService,
    ISqlSugarClient context) : BaseService<Advertises>(context), IAdvertiseService
{
    /// <inheritdoc/>
    public Task CreateAdvertise(Message message)
    {
        var newAd = new Advertises {
            ChatID = message.Chat.Id,
            MessageID = message.MessageId,
            Enable = false,
            PinMessage = false,
            Mode = EAdMode.All,
            Weight = 100,
            LastPostAt = DateTime.MinValue,
            ShowCount = 0,
            MaxShowCount = 0,
            ExternalLink = "",
            ExternalLinkName = "",
            CreateAt = DateTime.Now,
            ExpiredAt = DateTime.Now.AddDays(90),
        };

        return Insertable(newAd).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task<Advertises?> GetPostableAdvertise()
    {
        var ads = await Queryable()
            .Where(static x => x.Enable)
            .OrderBy(static x => x.Weight)
            .ToListAsync().ConfigureAwait(false);

        var now = DateTime.Now;
        //检查是否过期
        foreach (var ad in ads)
        {
            if ((ad.MaxShowCount > 0 && ad.ShowCount >= ad.MaxShowCount) ||
                now >= ad.ExpiredAt || ad.Weight == 0)
            {
                ad.Enable = false;
                await Updateable(ad).UpdateColumns(static x => new { x.Enable }).ExecuteCommandAsync().ConfigureAwait(false);
                //删除过期广告
                await _advertisePostService.DeleteOldAdPosts(ad).ConfigureAwait(false);
            }
        }

        now = now.AddSeconds(-now.Second).AddMinutes(-now.Minute).AddHours(-now.Hour);
        var validAds = ads.Where(x => x.Enable && x.LastPostAt < now);

        if (!validAds.Any())
        {
            return null;
        }

        //随机抽取广告
        int sum = 0;
        var weightList = new List<(int weight, Advertises ad)>();

        foreach (var ad in validAds)
        {
            sum += ad.Weight;
            weightList.Add((sum, ad));
        }

        int randInt = new Random().Next(0, sum);

        var randomAd = weightList.First(kv => kv.weight > randInt).ad;

        return randomAd;
    }

    /// <inheritdoc/>
    public Task UpdateAdvertiseStatistics(Advertises ad)
    {
        return Updateable(ad).UpdateColumns(static x => new {
            x.ShowCount, x.LastPostAt
        }).ExecuteCommandAsync();
    }
}
