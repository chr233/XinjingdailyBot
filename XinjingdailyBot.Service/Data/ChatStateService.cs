using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data
{
    [AppService(typeof(IChatStateService), LifeTime.Transient)]
    public sealed class ChatStateService : BaseService<ChatState>, IChatStateService
    {
        public bool If()
        {
            return true;
        }
    }
}
