using Injectio.Attributes;
using SqlSugar;
using XinjingdailyBot.Entry.Entries.User.Rbac;
using XinjingdailyBot.Repository.User.Rbac;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository.User.Rbac;

/// <summary>
/// 用户角色仓储实现
/// </summary>
[RegisterClass(ServiceLifetime.Scoped)]
public class UserRolesRepository : Repository<UserRoles>, IUserRolesRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public UserRolesRepository(ISqlSugarClient db) : base(db)
    {
    }
}
