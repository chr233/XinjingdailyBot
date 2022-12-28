using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Service.Data
{
    [AppService(ServiceType = typeof(IBanRecordService), ServiceLifetime = LifeTime.Transient)]
    public sealed class BanRecordService : BaseService<BanRecords>, IBanRecordService
    {
    }
}
