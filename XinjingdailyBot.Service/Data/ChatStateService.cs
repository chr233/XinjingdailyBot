using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;

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
