using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Data
{
    [AppService(typeof(IBanRecordService), LifeTime.Transient)]
    public sealed class BanRecordService : BaseService<BanRecords>, IBanRecordService
    {
    }
}
