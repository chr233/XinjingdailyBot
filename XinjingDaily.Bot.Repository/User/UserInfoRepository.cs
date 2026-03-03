using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.IRepository.User;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.User;

/// <summary>
/// 用户信息仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class UserInfoRepository(ISqlSugarClient _db) : RepositoryInt<UserInfo>(_db), IUserInfoRepository
{
    public async Task<UserInfo?> QueryByTelegramIdAsync(long telegramId)
    {
        return await _db
            .Queryable<UserInfo>()
            .FirstAsync(u => u.TelegramId == telegramId)
            .ConfigureAwait(false);
    }

    public async Task<UserInfo?> QueryByTelegramNameAsync(string telegramName)
    {
        return await _db
            .Queryable<UserInfo>()
            .FirstAsync(u => u.TelegramName != null && u.TelegramName == telegramName)
            .ConfigureAwait(false);
    }
}
