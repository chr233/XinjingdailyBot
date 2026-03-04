using XinjingDaily.Bot.Entry.Entries.Users.Rbac;
using XinjingDaily.Bot.IRepository.Base;

namespace XinjingDaily.Bot.IRepository.User.Rbac;

/// <summary>
/// 角色仓储接口
/// </summary>
public interface IRoleRepository : IRepositoryInt<Role>
{
    Task<List<Role>> QueryDefaultAdminRolesAsync();
    Task<List<Role>> QueryDefaultUserRolesAsync();
}
