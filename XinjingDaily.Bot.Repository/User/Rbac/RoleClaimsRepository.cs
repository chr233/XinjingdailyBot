using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Users.Rbac;
using XinjingDaily.Bot.IRepository.User.Rbac;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.User.Rbac;

/// <summary>
/// 角色权限仓储实现
/// </summary>
[RegisterScoped]
public class RoleClaimsRepository : RepositoryInt<RoleClaims>, IRoleClaimsRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public RoleClaimsRepository(ISqlSugarClient db) : base(db)
    {
    }
}
