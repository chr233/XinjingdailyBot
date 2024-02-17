using SqlSugar;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data;

[AppService(typeof(IStateContextService), LifeTime.Transient)]
public sealed class StateContextService(ISqlSugarClient context) : BaseService<StateContext>(context), IStateContextService
{
    /// <inheritdoc/>
    public Task<StateContext> GetContext(int userId)
    {
        return GetFirstAsync(x => x.UserId == userId);
    }

    /// <inheritdoc/>
    public Task ResetContext(int userId)
    {
        return DeleteAsync(x => x.UserId == userId);
    }

    /// <inheritdoc/>
    public async Task SetContext(int userId, string context)
    {
        await DeleteAsync(x => x.UserId == userId);

        await InsertAsync(new StateContext {
            UserId = userId,
            Context = context
        });
    }
}
