using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Chat;
using XinjingDaily.Bot.IRepository.Channel;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Channel;

/// <summary>
/// 频道信息仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class ChannelInfoRepository(ISqlSugarClient _db) : RepositoryInt<ChatInfo>(_db), IChatInfoRepository
{
    public async Task<ChatInfo?> QueryByTelegramIdAsync(long telegramId)
    {
        return await _db.Queryable<ChatInfo>()
            .Where(c => c.TelegramId == telegramId)
            .FirstAsync()
            .ConfigureAwait(false);
    }

    public async Task<ChatInfo?> QueryByTelegramNameAsync(string telegramName)
    {
        return await _db.Queryable<ChatInfo>()
            .Where(c => c.TelegramName != null && c.TelegramName == telegramName)
            .FirstAsync()
            .ConfigureAwait(false);
    }
}
