using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data
{
    [AppService(typeof(INameHistoryService), LifeTime.Singleton)]
    public sealed class NameHistoryService : BaseService<NameHistory>, INameHistoryService
    {
        public async Task CreateNameHistory(Users dbUser)
        {
            var history = new NameHistory
            {
                UId = dbUser.Id,
                FirstName = dbUser.FirstName,
                LastName = dbUser.LastName,
                CreateAt = DateTime.Now,
            };
            await InsertAsync(history);
        }
    }
}
