using XinjingDaily.Bot.Entry.Entries.Context;
using XinjingDaily.Bot.IRepository.Base;

namespace XinjingDaily.Bot.IRepository.Context;

public interface IUserContextRepository : IRepositoryInt<UserContextEntry>
{
    Task<UserContextEntry?> QueryAsync(int userId, long chatId);
    Task UpsertAsync(UserContextEntry entry);
}
