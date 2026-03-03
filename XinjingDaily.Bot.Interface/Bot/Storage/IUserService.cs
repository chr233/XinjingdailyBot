using Telegram.Bot.Types;
using XinjingDaily.Bot.Entry.Entries.Users;

namespace XinjingDaily.Bot.Interface.Bot.Storage;

public interface IUserService
{
    Task<UserInfo?> QueryUserFromUpdate(Update update);
}