using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.IRepository.User;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.User;

/// <summary>
/// 用户信息仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class UserInfoRepository : RepositoryInt<UserInfo>, IUserInfoRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public UserInfoRepository(ISqlSugarClient db) : base(db)
    {
    }
}
