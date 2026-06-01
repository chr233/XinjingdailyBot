using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Context;
using XinjingDaily.Bot.IRepository.Context;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Context;

[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class ChatContextRepository(ISqlSugarClient db)
    : RepositoryInt<ChatContextEntry>(db), IChatContextRepository
{
    public async Task<ChatContextEntry?> QueryAsync(string command, long chatId)
        => await Queryable()
            .FirstAsync(x => x.Command == command && x.ChatId == chatId)
            .ConfigureAwait(false);

    public async Task UpsertAsync(ChatContextEntry entry)
    {
        entry.ModifyAt = DateTime.UtcNow;
        if (entry.Id == 0)
        {
            entry.Id = await Insertable(entry)
                .ExecuteReturnIdentityAsync()
                .ConfigureAwait(false);
        }
        else
        {
            await Updateable(entry)
                .UpdateColumns(static e => new { e.Mode, e.DataJson, e.ModifyAt })
                .ExecuteCommandAsync()
                .ConfigureAwait(false);
        }
    }
}
