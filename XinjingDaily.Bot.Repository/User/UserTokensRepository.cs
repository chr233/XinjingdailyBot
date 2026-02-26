using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.IRepository.User;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.User;

/// <summary>
/// 用户令牌仓储实现
/// </summary>
[RegisterScoped]
public class UserTokensRepository : RepositoryInt<UserTokens>, IUserTokenRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public UserTokensRepository(ISqlSugarClient db) : base(db)
    {
    }
}
