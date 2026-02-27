using Injectio.Attributes;
using SqlSugar;
using XinjingdailyBot.Entry.Entries.User.Rbac;
using XinjingdailyBot.Repository.User.Rbac;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository.User.Rbac;

/// <summary>
/// 用户权限仓储实现
/// </summary>
[RegisterClass(ServiceLifetime.Scoped)]
public class UserClaimsRepository : Repository<UserClaims>, IUserClaimsRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public UserClaimsRepository(ISqlSugarClient db) : base(db)
    {
    }
}
