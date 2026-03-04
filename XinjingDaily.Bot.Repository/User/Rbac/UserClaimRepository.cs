using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.Entry.Entries.Users.Rbac;
using XinjingDaily.Bot.IRepository.User.Rbac;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.User.Rbac;

/// <summary>
/// 权限仓储实现
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="db"></param>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class UserClaimRepository(ISqlSugarClient _db) : RepositoryInt<UserClaim>(_db), IUserClaimRepository
{
    public async Task<List<string?>> QueryUserClaimsAsync(UserInfo userInfo)
    {
        // 1. 定义直接权限子查询：UserClaim -> Claim
        return await Queryable()
            .Where(uc => uc.UserId == userInfo.Id)
            .LeftJoin<Claim>((uc, c) => uc.ClaimId == c.Id)
            .Select((uc, c) => c.Key)
            .Where(uc => !string.IsNullOrEmpty(uc))
            .Distinct()
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
