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

    public async Task<List<string>> QueryUserRoleClaimsAsync(UserInfo userInfo)
    {
        return await Queryable()
            .Where(ur => ur.UserId == userInfo.Id)
            .LeftJoin<Role>((ur, r) => ur.RoleId == r.Id)
            .LeftJoin<RoleClaim>((ur, r, rc) => r.Id == rc.RoleId)
            .LeftJoin<Claim>((ur, r, rc, c) => rc.ClaimId == c.Id)
            .Select((ur, r, rc, c) => SqlFunc.ToUpper(c.Value ?? ""))
            .Distinct()
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
