using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Interface;
using Telegram.Bot.Types;

namespace XinjingdailyBot.Interface.Data
{
    public interface IUserService : IBaseService<Users>
    {
        Task<Users?> FetchUser(Update update);
    }
}
