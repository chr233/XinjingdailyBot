using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data;

/// <summary>
/// 用户Token仓储服务
/// </summary>
public interface IUserTokenService : IBaseService<UserTokens>
{
    /// <summary>
    /// Token过期最大时间
    /// </summary>
    public static DateTime MaxExpiredValue => new DateTime(9999, 12, 31, 23, 59, 59);

    /// <summary>
    /// 获取已有的Token或者生成新的Token
    /// </summary>
    /// <param name="dbUser"></param>
    /// <returns></returns>
    Task<UserTokens> GenerateNewUserToken(Users dbUser);
    /// <summary>
    /// 获取已有的Token
    /// </summary>
    /// <param name="dbUser"></param>
    /// <returns></returns>
    Task<UserTokens?> FetchUserToken(Users dbUser);
    /// <summary>
    /// 验证Token是否有效
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Users?> VerifyToken(Guid token);
}
