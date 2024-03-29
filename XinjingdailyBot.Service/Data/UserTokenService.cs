using Microsoft.Extensions.Logging;
using SqlSugar;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data;

/// <inheritdoc cref="IUserTokenService"/>
[AppService(typeof(IUserTokenService), LifeTime.Singleton)]
public sealed class UserTokenService(
    ILogger<UserTokenService> _logger,
    ISqlSugarClient context) : BaseService<UserTokens>(context), IUserTokenService
{

    /// <inheritdoc/>
    public async Task<UserTokens> GenerateNewUserToken(Users dbUser)
    {
        var token = await Queryable().Where(x => x.UID == dbUser.Id).FirstAsync().ConfigureAwait(false);
        if (token == null)
        {
            token = new UserTokens {
                UID = dbUser.Id,
                UserID = dbUser.UserID,
                APIToken = Guid.NewGuid(),
                ExpiredAt = IUserTokenService.MaxExpiredValue,
            };
            await Insertable(token).ExecuteCommandAsync().ConfigureAwait(false);
        }
        else
        {
            token.APIToken = Guid.NewGuid();
            await Updateable(token).ExecuteCommandAsync().ConfigureAwait(false);
        }

        _logger.LogInformation("为 {user} 生成新 Token {token}", dbUser, token.APIToken);

        return token;
    }

    /// <inheritdoc/>
    public async Task<UserTokens?> FetchUserToken(Users dbUser)
    {
        var token = await Queryable().Where(x => x.UID == dbUser.Id).FirstAsync().ConfigureAwait(false);
        if (token == null || token.ExpiredAt < DateTime.Now)
        {
            return null;
        }
        return token;
    }

    /// <inheritdoc/>
    public async Task<Users?> VerifyToken(Guid token)
    {
        var userToken = await Queryable()
            .Includes(static x => x.User)
            .FirstAsync(x => x.APIToken == token).ConfigureAwait(false);

        if (userToken?.User != null && userToken.ExpiredAt > DateTime.Now)
        {
            return userToken.User;
        }

        return null;
    }
}
