using SqlSugar;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data;

/// <inheritdoc cref="IBanRecordService"/>
[AppService(typeof(IBanRecordService), LifeTime.Transient)]
internal sealed class BanRecordService : BaseService<BanRecords>, IBanRecordService
{
    public BanRecordService(ISqlSugarClient context) : base(context)
    {
    }
}
