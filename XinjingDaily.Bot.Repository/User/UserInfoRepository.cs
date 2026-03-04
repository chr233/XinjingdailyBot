using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.Entry.Entries.Users.Rbac;
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
        return await Queryable()
            .FirstAsync(u => u.TelegramId == telegramId)
            .ConfigureAwait(false);
    }

    public async Task<UserInfo?> QueryByTelegramNameAsync(string telegramName)
    {
        return await Queryable()
            .FirstAsync(u => u.TelegramName != null && u.TelegramName == telegramName)
            .ConfigureAwait(false);
    }

    private async Task UpdateNameAsync(UserInfo userInfo)
    {
        if (userInfo.TelegramName?.Length >= 150)
        {
            userInfo.TelegramName = userInfo.TelegramName[..150];
        }

        if (userInfo.FirstName?.Length >= 150)
        {
            userInfo.FirstName = userInfo.FirstName[..150];
        }

        if (userInfo.LastName?.Length >= 150)
        {
            userInfo.LastName = userInfo.LastName[..150];
        }

        userInfo.ModifyAt = DateTime.Now;
        await Updateable(userInfo)
            .UpdateColumns(static u => new { u.TelegramName, u.FirstName, u.LastName, u.ModifyAt })
            .ExecuteCommandAsync()
            .ConfigureAwait(false);
    }

    public async Task UpdateTelegramNameAndNickNameAsync(UserInfo userInfo)
    {
        if (!string.IsNullOrEmpty(userInfo.TelegramName))
        {
            // 如果有有重名的用户, 去掉重名用户的 TelegramName
            var existUser = await Queryable()
               .FirstAsync(u => u.TelegramName == userInfo.TelegramName)
               .ConfigureAwait(false);

            existUser.TelegramName = $"{existUser.TelegramName} [Dup {DateTime.Now}]";
            await UpdateNameAsync(existUser).ConfigureAwait(false);
        }

        await UpdateNameAsync(userInfo).ConfigureAwait(false);
    }

    public async Task UpdateIsBotAsync(UserInfo userInfo)
    {
        userInfo.ModifyAt = DateTime.Now;
        await Updateable(userInfo)
            .UpdateColumns(static u => new { u.IsBot, u.ModifyAt })
            .ExecuteCommandAsync()
            .ConfigureAwait(false);
    }

    public async Task UpdateModifyAsync(UserInfo userInfo)
    {
        userInfo.ModifyAt = DateTime.Now;
        await Updateable(userInfo)
            .UpdateColumns(static u => new { u.ModifyAt })
            .ExecuteCommandAsync()
            .ConfigureAwait(false);
    }

    public async Task<HashSet<string?>> QueryUserClaimsAsync(UserInfo userInfo)
    {
        var user = await Queryable()
            .Includes(u => u.UserClaims)
            .ToListAsync();

        

        // 合并去重
        return [];
    }
}
