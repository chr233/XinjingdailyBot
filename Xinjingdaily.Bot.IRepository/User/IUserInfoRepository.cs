using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.IRepository.Base;

namespace XinjingDaily.Bot.IRepository.User;

/// <summary>
/// 用户信息仓储接口
/// </summary>
public interface IUserInfoRepository : IRepositoryInt<UserInfo>
{
    Task<UserInfo?> QueryByTelegramIdAsync(long telegramId);
    Task<UserInfo?> QueryByTelegramNameAsync(string telegramName);
}
