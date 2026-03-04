using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.Entry.Entries.Users.Rbac;
using XinjingDaily.Bot.IRepository.Base;

namespace XinjingDaily.Bot.IRepository.User.Rbac;

/// <summary>
/// 用户权限仓储接口
/// </summary>
public interface IUserClaimRepository : IRepositoryInt<UserClaim>
{
    Task<List<string?>> QueryUserClaimsAsync(UserInfo userInfo);
}
