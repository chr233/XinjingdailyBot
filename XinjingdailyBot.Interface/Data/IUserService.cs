using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    public interface IUserService : IBaseService<Users>
    {
        Task<Users?> FetchTargetUser(Message message);
        Task<Users?> FetchUserByUserID(long userID);
        Task<Users?> FetchUserByUserName(string? userName);
        Task<Users?> FetchUserByUserNameOrUserID(string? target);
        Task<Users?> FetchUserFromUpdate(Update update);
        Task<(string, InlineKeyboardMarkup?)> QueryUserList(Users dbUser, string query, int page);
    }
}
