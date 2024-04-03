using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Data;
public interface IStateContextService
{
    Task<StateContext> GetContext(int userId);
    Task ResetContext(int userId);
}