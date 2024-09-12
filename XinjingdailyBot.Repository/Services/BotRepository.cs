using SqlSugar;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository.Services;

[AppService(LifeTime.Transient)]
public class BotRepository(ISqlSugarClient context) : BaseRepository<Bots>(context)
{
    public Task<Bots> CreateBot(bool enable, string botToken, byte weight)
    {
        var bot = new Bots {
            Enabled = enable,
            BotToken = botToken,
            Weight = weight,
            UserId = -1,
            Nickname = "",
            Username = "",
            CreateAt = DateTime.Now,
            ModifyAt = DateTime.Now,
        };

        return Insertable(bot).ExecuteReturnEntityAsync();
    }

    public async Task<Bots?> QueryBotById(int id)
    {
        return await Queryable()
            .Where(b => b.Id == id).FirstAsync().ConfigureAwait(false);
    }

    public Task<List<Bots>> QueryBotByName(string? botname, string? nickname, int page, int limit)
    {
        return Queryable()
            .WhereIF(!string.IsNullOrEmpty(botname), b => b.Username != null && b.Username.Contains(botname!))
            .WhereIF(!string.IsNullOrEmpty(nickname), b => b.Nickname != null && b.Nickname.Contains(nickname!))
            .ToPageListAsync(page, limit);
    }

    public Task<List<Bots>> QueryBotsEnabled()
    {
        return Queryable().Where(static x => x.Enabled).ToListAsync();
    }

    public Task UpdateBot(Bots bot)
    {
        bot.ModifyAt = DateTime.Now;
        return Updateable(bot).ExecuteCommandAsync();
    }

    public Task<bool> DeleteBot(int id)
    {
        return Deleteable().Where(x => x.Id == id).ExecuteCommandHasChangeAsync();
    }
}