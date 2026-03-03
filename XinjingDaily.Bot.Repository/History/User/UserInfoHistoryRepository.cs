using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.History.User;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.IRepository.History.User;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.History.User;

/// <summary>
/// 封禁历史仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class UserInfoHistoryRepository(ISqlSugarClient _db) : RepositoryInt<UserInfoHistory>(_db), IUserInfoHistoryRepository
{
    public async Task CreateHistoryAsync(UserInfo user, bool nickChanged, bool telegramNameChanged)
    {
        var history = new UserInfoHistory {
            UserId = user.Id,
            FirstName = nickChanged ? user.FirstName : null,
            LastName = nickChanged ? user.LastName : null,
            IsNickChanged = nickChanged,
            TelegramName = telegramNameChanged ? user.TelegramName : null,
            IsTelegramNameChanged = telegramNameChanged,
            CreateAt = DateTime.Now
        };

        await _db.Insertable(history).ExecuteCommandAsync().ConfigureAwait(false);
    }
}
