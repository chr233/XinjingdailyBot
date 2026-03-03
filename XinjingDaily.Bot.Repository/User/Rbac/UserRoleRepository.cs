using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Users.Rbac;
using XinjingDaily.Bot.IRepository.User.Rbac;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.User.Rbac;

/// <summary>
/// 权限仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class UserRoleRepository : RepositoryInt<UserRole>, IUserRoleRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public UserRoleRepository(ISqlSugarClient db) : base(db)
    {
    }
}
