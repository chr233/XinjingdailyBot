using XinjingDaily.Bot.Entry.Entries.Context;
using XinjingDaily.Bot.IRepository.Base;

namespace XinjingDaily.Bot.IRepository.Context;

public interface IChatContextRepository : IRepositoryInt<ChatContextEntry>
{
    Task<ChatContextEntry?> QueryAsync(string command, long chatId);
    Task UpsertAsync(ChatContextEntry entry);
}