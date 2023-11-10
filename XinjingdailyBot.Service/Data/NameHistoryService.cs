using SqlSugar;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data;

/// <inheritdoc cref="INameHistoryService"/>
[AppService(typeof(INameHistoryService), LifeTime.Singleton)]
internal sealed class NameHistoryService : BaseService<NameHistory>, INameHistoryService
{
    public NameHistoryService(ISqlSugarClient context) : base(context)
    {
    }

    public async Task CreateNameHistory(Users dbUser)
    {
        var history = new NameHistory {
            UId = dbUser.Id,
            FirstName = dbUser.FirstName,
            LastName = dbUser.LastName,
            CreateAt = DateTime.Now,
        };
        await InsertAsync(history);
    }
}
