using SqlSugar;
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
public class UserRoleRepository(ISqlSugarClient db) : RepositoryInt<UserRole>(db), IUserRoleRepository
{
    public async Task UpdateUserRolesAsync(int userId, List<int> roleIds)
    {
        // 删除用户现有的角色关联
        await Deleteable().Where(ur => ur.UserId == userId).ExecuteCommandAsync().ConfigureAwait(false);
        // 添加新的角色关联
        var userRoles = roleIds.Select(roleId => new UserRole { UserId = userId, RoleId = roleId }).ToList();
        await Insertable(userRoles).ExecuteCommandAsync().ConfigureAwait(false);
    }
}
