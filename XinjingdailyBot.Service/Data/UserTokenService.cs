using Microsoft.Extensions.Logging;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data
{
    [AppService(typeof(IUserTokenService), LifeTime.Singleton)]
    public sealed class UserTokenService : BaseService<UserTokens>, IUserTokenService
    {
        private readonly ILogger<UserTokenService> _logger;

        public UserTokenService(ILogger<UserTokenService> logger)
        {
            _logger = logger;
        }

        public async Task<UserTokens?> GenerateNewUserToken(Users dbUser)
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
    }
}
