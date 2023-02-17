using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Data
{
    [AppService(typeof(IDialogueService), LifeTime.Transient)]
    public sealed class DialogueService : BaseService<Dialogue>, IDialogueService
    {
        public async Task RecordUpdate(Update update)
        {
            
        }
    }
}
