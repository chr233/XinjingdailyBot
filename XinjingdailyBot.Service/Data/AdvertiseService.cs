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
        IAdvertisePostService advertisePostService)
    {
        _advertisePostService = advertisePostService;
    }

    public async Task<Advertises?> GetPostableAdvertise()
    {
        var ads = await Queryable().Where(static x => x.Enable).ToListAsync();

        var now = DateTime.Now;
        //检查是否过期
        foreach (var ad in ads)
        {
            if ((ad.MaxShowCount > 0 && ad.ShowCount >= ad.MaxShowCount) ||
                now >= ad.ExpireAt || ad.Weight == 0)
            {
                ad.Enable = false;
                await UpdateAsync(ad);
                await _advertisePostService.DeleteOldAdPosts(ad, false);
            }
        }
        var validAds = ads.Where(static x => x.Enable);

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
