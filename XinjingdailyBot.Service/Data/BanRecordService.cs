using SqlSugar;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data;

/// <inheritdoc cref="IBanRecordService"/>
[AppService(typeof(IBanRecordService), LifeTime.Transient)]
public sealed class BanRecordService(ISqlSugarClient context) : BaseService<BanRecords>(context), IBanRecordService
{
    /// <inheritdoc/>
    public async Task AddBanRecord(Users targetUser, Users operatorUser, EBanType banType, string reason)
    {
        var record = new BanRecords {
            UserID = targetUser.UserID,
            OperatorUID = operatorUser.UserID,
            Type = banType,
            BanTime = DateTime.Now,
            Reason = reason,
        };

        await Insertable(record).ExecuteCommandAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<int> GetWarnCount(Users targetUser)
    {
        //获取最近一条解封记录
        var lastUnbaned = await GetLatestBanRecord(targetUser.UserID).ConfigureAwait(false);

        var expireTime = DateTime.Now.AddDays(-IBanRecordService.WarnDuration);

        int warnCount = await Queryable()
                        .Where(x => x.UserID == targetUser.UserID && x.Type == EBanType.Warning && x.BanTime > expireTime)
                        .WhereIF(lastUnbaned != null, x => x.BanTime >= lastUnbaned!.BanTime)
                        .CountAsync().ConfigureAwait(false);

        return warnCount;
    }

    /// <inheritdoc/>
    public Task<List<BanRecords>> GetBanRecores(Users targetUser)
    {
        return Queryable()
         .Where(x => x.UserID == targetUser.UserID)
         .OrderByDescending(static x => new { x.BanTime }).ToListAsync();
    }

    /// <inheritdoc/>
    public Task<List<BanRecords>> GetBanRecores(Users targetUser, DateTime expireTime)
    {
        return Queryable()
            .Where(x => x.UserID == targetUser.UserID && (x.Type != EBanType.Warning || x.BanTime > expireTime))
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<BanRecords?> GetLatestBanRecord(long userId)
    {
        return await Queryable()
                .Where(x => x.UserID == userId && (x.Type == EBanType.UnBan || x.Type == EBanType.Ban))
                .OrderByDescending(static x => x.BanTime).FirstAsync().ConfigureAwait(false);
    }
}
