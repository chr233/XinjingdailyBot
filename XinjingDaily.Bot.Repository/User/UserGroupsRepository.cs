using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.User.Group;
using XinjingDaily.Bot.IRepository.User;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.User;

/// <summary>
/// 用户组仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class UserGroupsRepository : RepositoryInt<UserGroup>, IUserGroupRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public UserGroupsRepository(ISqlSugarClient db) : base(db)
    {
    }
}
