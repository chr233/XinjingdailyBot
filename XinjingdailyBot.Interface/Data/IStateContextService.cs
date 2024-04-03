using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data;
public interface IStateContextService
{
    StateContext? GetContext(int userId);
}