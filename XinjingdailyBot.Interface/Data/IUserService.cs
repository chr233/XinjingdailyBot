using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    public interface IUserService : IBaseService<Users>
    {
        Task<Users?> FetchUser(Update update);
    }
}
