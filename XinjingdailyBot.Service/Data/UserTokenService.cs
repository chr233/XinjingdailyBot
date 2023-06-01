using Microsoft.Extensions.Logging;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data
{
    /// <inheritdoc cref="IUserTokenService"/>
    [AppService(typeof(IUserTokenService), LifeTime.Singleton)]
    internal sealed class UserTokenService : BaseService<UserTokens>, IUserTokenService
    {
        private readonly ILogger<UserTokenService> _logger;
        private readonly IUserService _userService;

        public UserTokenService(
            ILogger<UserTokenService> logger,
            IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public async Task<UserTokens> GenerateNewUserToken(Users dbUser)
        {
            var token = await Queryable().Where(x => x.UserID == dbUser.UserID).FirstAsync();
            if (token == null)
            {
                token = new UserTokens {
                    UserID = dbUser.UserID,
                    APIToken = Guid.NewGuid(),
                    ExpiredAt = IUserTokenService.MaxExpiredValue,
                };
                await Insertable(token).ExecuteCommandAsync();
            }
            else
            {
                token.APIToken = Guid.NewGuid();
                await Updateable(token).ExecuteCommandAsync();
            }

            _logger.LogInformation("为 {user} 生成新 Token {token}", dbUser, token.APIToken);

            return token;
        }

        public async Task<UserTokens?> FetchUserToken(Users dbUser)
        {
            var token = await Queryable().Where(x => x.UserID == dbUser.UserID).FirstAsync();
            if (token == null || token.ExpiredAt < DateTime.Now)
            {
                return null;
            }
            return token;
        }

        public async Task<Users?> VerifyToken(Guid token)
        {
            var userToken = await Queryable().Where(x => x.APIToken == token).FirstAsync();
            if (userToken == null || userToken.ExpiredAt < DateTime.Now)
            {
                return null;
            }

            var user = await _userService.FetchUserByUserID(userToken.UserID);
            return user;
        }
    }
}
