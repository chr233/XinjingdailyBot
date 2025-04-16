using SqlSugar;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Model.Legacy;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository.Services;

[AppService(LifeTime.Transient)]
public class BotRepository(ISqlSugarClient context) : BaseRepository<BotsLegacy>(context)
{
    public Task<BotsLegacy> CreateBot(bool enable, string botToken, byte weight)
    {
        var bot = new BotsLegacy {
            Enabled = enable,
            BotToken = botToken,
            Weight = weight,
            UserId = -1,
            Firstname = null,
            Username = null,
            CreateAt = DateTime.Now,
            ModifyAt = DateTime.Now,
        };

        return Insertable(bot).ExecuteReturnEntityAsync();
    }

    public async Task<BotsLegacy?> QueryBotById(int id)
    {
        return await Queryable()
            .Where(b => b.Id == id).FirstAsync().ConfigureAwait(false);
    }

    public Task<List<BotsLegacy>> QueryBotByName(string? botname, string? nickname, int page, int limit)
    {
        return Queryable()
            .WhereIF(!string.IsNullOrEmpty(botname), b => b.Username != null && b.Username.Contains(botname!))
            .WhereIF(!string.IsNullOrEmpty(nickname), b => b.Firstname != null && b.Firstname.Contains(nickname!))
            .ToPageListAsync(page, limit);
    }

    public Task<List<BotsLegacy>> QueryBotsEnabled()
    {
        return Queryable().Where(static x => x.Enabled).ToListAsync();
    }

    public Task UpdateBot(BotsLegacy bot)
    {
        bot.ModifyAt = DateTime.Now;
        return Updateable(bot).ExecuteCommandAsync();
    }

    public Task<bool> DeleteBot(int id)
    {
        return Deleteable().Where(x => x.Id == id).ExecuteCommandHasChangeAsync();
    }
}