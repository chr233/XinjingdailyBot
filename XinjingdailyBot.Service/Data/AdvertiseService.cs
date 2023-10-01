using SqlSugar;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data;

/// <inheritdoc cref="IAdvertiseService"/>
[AppService(typeof(IAdvertiseService), LifeTime.Transient)]
internal sealed class AdvertiseService : BaseService<Advertises>, IAdvertiseService
{
    private readonly IAdvertisePostService _advertisePostService;

    public AdvertiseService(
        IAdvertisePostService advertisePostService,
        ISqlSugarClient context) : base(context)
    {
        _advertisePostService = advertisePostService;
    }

    public async Task DisableExpiredAdvertise()
    {
        var ads = await Queryable()
           .Where(static x => x.Enable)
           .OrderBy(static x => x.Weight)
           .ToListAsync();

        var now = DateTime.Now;
        //检查是否过期
        foreach (var ad in ads)
        {
            if ((ad.MaxShowCount > 0 && ad.ShowCount >= ad.MaxShowCount) ||
                now >= ad.ExpiredAt || ad.Weight == 0)
            {
                ad.Enable = false;
                await Updateable(ad).UpdateColumns(static x => new { x.Enable }).ExecuteCommandAsync();
                await _advertisePostService.DeleteOldAdPosts(ad);
            }
        }
    }

    public async Task<Advertises?> GetPostableAdvertise()
    {
        var ads = await Queryable()
            .Where(static x => x.Enable)
            .OrderBy(static x => x.Weight)
            .ToListAsync();

        var now = DateTime.Now;
        //检查是否过期
        foreach (var ad in ads)
        {
            if ((ad.MaxShowCount > 0 && ad.ShowCount >= ad.MaxShowCount) ||
                now >= ad.ExpiredAt || ad.Weight == 0)
            {
                ad.Enable = false;
                await Updateable(ad).UpdateColumns(static x => new { x.Enable }).ExecuteCommandAsync();
                await _advertisePostService.DeleteOldAdPosts(ad);
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
}
