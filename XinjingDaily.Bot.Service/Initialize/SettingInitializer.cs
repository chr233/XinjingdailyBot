using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XinjingDaily.Bot.Entry.Columns;
using XinjingDaily.Bot.Entry.Entries.Users.Rbac;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Interface.InitService;
using XinjingDaily.Bot.IRepository.User.Rbac;

namespace XinjingDaily.Bot.Service.InitService;

/// <summary>
/// 默认数据初始化服务
/// </remarks>
/// <param name="_logger"></param>
[RegisterTransient<IServiceInitializer>(Duplicate = DuplicateStrategy.Append, Registration = RegistrationStrategy.ImplementedInterfaces)]
public class SettingInitializer(
    ILogger<SettingInitializer> _logger,
    IOptions<AppSettings> _options,
    IClaimRepository _claimRepository,
    IRoleClaimRepository _roleClaimRepository,
    IRoleRepository _roleRepository,
    IUserRoleRepository _userRoleRepository,
    IUserClaimRepository _userClaimRepository,
    IServiceProvider _serviceProvider
) : IServiceInitializer
{
    /// <inheritdoc/>
    public int Order => 4;

    private sealed record RoleClaimDefinition(int Id, string RoleName, string RoleDescriptions, List<int>? ClaimKeys, bool IsDefaultUserRole = false);

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        List<Claim> claims = [
            new Claim { Id = 1, Name = "查询自己的信息", Description = "", Value = "query:self:command" },
            new Claim { Id = 2, Name = "查询所有人的信息", Description = "", Value = "query:all:command" },
            new Claim { Id = 1, Name = "一般功能命令", Description = "", Value = "common:command" },
            new Claim { Id = 1, Name = "群管功能命令", Description = "", Value = "group-admin:command" },
            new Claim { Id = 1, Name = "投稿功能命令", Description = "", Value = "group-admin:command" },
            new Claim { Id = 2, Name = "功能命令", Description = "", Value = "function:command" },
            new Claim { Id = 3, Name = "创建投稿", Description = "", Value = "post:create" },
            new Claim { Id = 4, Name = "执行审核命令", Description = "", Value = "review:command" },
            new Claim { Id = 5, Name = "执行审核命令", Description = "", Value = "review:command" },
            new Claim { Id = 6, Name = "执行审核命令", Description = "", Value = "review:command" },
            new Claim { Id = 7, Name = "用户管理", Description = "", Value = "UserManagement" },
            new Claim { Id = 8, Name = "角色管理", Description = "", Value = "RoleManagement" },
        ];

        claims.ForEach(static x => x.Value = x.Value?.ToUpperInvariant());

        List<RoleClaimDefinition> roleDefinition = [
            new RoleClaimDefinition ( 1,"受限用户", "封禁用户", [1] ,true) ,
            new RoleClaimDefinition ( 2,"初级用户", "默认角色, 未投稿过的用户", [1] ,true) ,
            new RoleClaimDefinition ( 3,"普通用户", "投稿过的用户", [2,3,4] ),
            new RoleClaimDefinition ( 4,"高级投稿用户", "无视投稿数量限制", [6,8,7] ),
            new RoleClaimDefinition ( 5,"群聊管理员", "允许使用群管理命令", [3] ),
            new RoleClaimDefinition ( 6,"投稿审核员", "允许使用投稿审核功能", [6] ),
            new RoleClaimDefinition ( 7,"频道管理员", "允许设置发布频道", [4] ),
            new RoleClaimDefinition ( 8,"机器人管理员", "允许修改机器人设置", [6] ),
            new RoleClaimDefinition ( 9,"超级管理员", "最高的权限", [5] ),
        ];

        var roles = roleDefinition
            .Select(static x => new Role { Id = x.Id, Name = x.RoleName, Description = x.RoleDescriptions, IsDefaultUserRole = x.IsDefaultUserRole })
            .ToList();

        var roleClaims = roleDefinition
            .SelectMany(static x => (x.ClaimKeys ?? []).Select(claimId => new RoleClaim {
                RoleId = x.Id,
                ClaimId = claimId
            }))
            .Distinct()
            .ToList();

        await InitDefaultClaims(claims).ConfigureAwait(false);
        await InitDefaultRoles(roles).ConfigureAwait(false);
        await InitDefaultRoleClaims(roleClaims).ConfigureAwait(false);
    }


    #region 数据库操作
    /// <summary>
    /// 插入数据库
    /// </summary>
    /// <typeparam name="TRepository"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="repo"></param>
    /// <param name="entities"></param>
    /// <returns></returns>
    private async Task InsertToTableInt<TRepository, TEntity>(TRepository repo, List<TEntity> entities)
        where TRepository : IRepository.Base.IRepositoryInt<TEntity>
        where TEntity : class, new()
    {
        await InsertToTable<TRepository, TEntity, int>(repo, entities).ConfigureAwait(false);
    }

    /// <summary>
    /// 插入数据库
    /// </summary>
    /// <typeparam name="TRepository"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="repo"></param>
    /// <param name="entities"></param>
    /// <returns></returns>
    private async Task InsertToTable<TRepository, TEntity, TKey>(TRepository repo, List<TEntity> entities)
        where TRepository : IRepository.Base.IRepository<TEntity, TKey>
        where TEntity : class, new()
    {
        if (entities.Count == 0)
        {
            _logger.LogDebug("没有需要插入的数据, 跳过插入");
            return;
        }

        var create = DateTime.UtcNow;
        var modify = DateTime.MinValue;

        foreach (var entity in entities)
        {
            if (typeof(TEntity) is ICreateAt ca)
            {
                ca.CreateAt = create;
            }
            if (typeof(TEntity) is IModifyAt ma)
            {
                ma.ModifyAt = modify;
            }
        }

        await repo.InsertOrUpdateAsync(entities).ConfigureAwait(false);
    }

    private async Task ClearTableInt<TRepository, TEntity>(TRepository repo)
        where TRepository : IRepository.Base.IRepositoryInt<TEntity>
        where TEntity : class, new()
    {
        await ClearTable<TRepository, TEntity, int>(repo).ConfigureAwait(false);
    }

    private async Task ClearTable<TRepository, TEntity, TKey>(TRepository repo)
        where TRepository : IRepository.Base.IRepository<TEntity, TKey>
        where TEntity : class, new()
    {
        var entries = await repo.QueryAllAsync().ConfigureAwait(false);
        await repo.DeleteAsync(entries).ConfigureAwait(false);
    }
    #endregion

    /// <summary>
    /// 初始化权限字段
    /// </summary>
    /// <returns></returns>
    private async Task InitDefaultClaims(List<Claim> claims)
    {
        ClearTableInt<IClaimRepository, Claim>(_claimRepository).ConfigureAwait(false).GetAwaiter().GetResult();

        if (await _claimRepository.CountAsync().ConfigureAwait(false) > 0)
        {
            _logger.LogDebug("已存在权限数据, 跳过初始化");
#if RELEASE
            return;
#endif
        }

        await InsertToTableInt(_claimRepository, claims).ConfigureAwait(false);
    }

    /// <summary>
    /// 初始化角色字段
    /// </summary>
    /// <returns></returns>
    private async Task InitDefaultRoles(List<Role> roles)
    {
        ClearTableInt<IRoleRepository, Role>(_roleRepository).ConfigureAwait(false).GetAwaiter().GetResult();

        if (await _roleRepository.CountAsync().ConfigureAwait(false) > 0)
        {
            _logger.LogDebug("已存在权限数据, 跳过初始化");
#if RELEASE
            return;
#endif
        }

        await InsertToTableInt(_roleRepository, roles).ConfigureAwait(false);
    }


    /// <summary>
    /// 初始化角色字段
    /// </summary>
    /// <returns></returns>
    private async Task InitDefaultRoleClaims(List<RoleClaim> roleClaims)
    {
        ClearTableInt<IRoleClaimRepository, RoleClaim>(_roleClaimRepository).ConfigureAwait(false).GetAwaiter().GetResult();

        if (await _roleClaimRepository.CountAsync().ConfigureAwait(false) > 0)
        {
            _logger.LogDebug("已存在权限数据, 跳过初始化");
#if RELEASE
            return;
#endif
        }

        await InsertToTableInt(_roleClaimRepository, roleClaims).ConfigureAwait(false);
    }


}
