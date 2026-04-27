using XinjingDaily.Bot.Entry.Entries.Users.Rbac;
using XinjingDaily.Bot.IRepository.Base;

namespace XinjingDaily.Bot.IRepository.User.Rbac;

/// <summary>
/// 权限仓储接口
/// </summary>
public interface IClaimRepository : IRepositoryInt<Claim>
{
    Task<List<string>?> QueryAllClaimsAsync();
}
