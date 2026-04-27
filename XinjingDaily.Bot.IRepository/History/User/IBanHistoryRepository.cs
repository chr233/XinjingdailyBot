using XinjingDaily.Bot.Entry.Entries.History.User;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.IRepository.Base;

namespace XinjingDaily.Bot.IRepository.History.User;

/// <summary>
/// 封禁历史仓储接口
/// </summary>
public interface IUserInfoHistoryRepository : IRepositoryInt<UserInfoHistory>
{
    Task CreateHistoryAsync(UserInfo user, bool nickChanged, bool telegramNameChanged);
}
