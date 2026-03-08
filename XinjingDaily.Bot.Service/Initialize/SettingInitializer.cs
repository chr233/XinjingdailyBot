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
[RegisterTransient<IServiceInitializer>(
    Duplicate = DuplicateStrategy.Append,
    Registration = RegistrationStrategy.ImplementedInterfaces
)]
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

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        await InitDefaultClaims().ConfigureAwait(false);
        await InitDefaultRoles().ConfigureAwait(false);
        await InitDefaultRoleClaims().ConfigureAwait(false);
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

        await repo.InsertAsync(entities).ConfigureAwait(false);
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
    private async Task InitDefaultClaims()
    {
        ClearTableInt<IClaimRepository, Claim>(_claimRepository).ConfigureAwait(false).GetAwaiter().GetResult();

        if (await _claimRepository.CountAsync().ConfigureAwait(false) > 0)
        {
            _logger.LogDebug("已存在权限数据, 跳过初始化");
            return;
        }

        List<Claim> claims =
        [
            new Claim { Id = 1, Name = "机器人管理命令", Value = "bot:admin:command" },
            new Claim { Id = 1, Name = "执行审核命令", Value = "review:command" },
            new Claim { Id = 1, Name = "执行审核命令", Value = "review:command" },
            new Claim { Id = 1, Name = "执行审核命令", Value = "review:command" },
            new Claim { Id = 2, Name = "用户管理", Value = "UserManagement" },
            new Claim { Id = 3, Name = "角色管理", Value = "RoleManagement" },
        ];

        await InsertToTableInt(_claimRepository, claims).ConfigureAwait(false);
    }

    /// <summary>
    /// 初始化角色字段
    /// </summary>
    /// <returns></returns>
    private async Task InitDefaultRoles()
    {
        ClearTableInt<IRoleRepository, Role>(_roleRepository).ConfigureAwait(false).GetAwaiter().GetResult();

        if (await _roleRepository.CountAsync().ConfigureAwait(false) > 0)
        {
            _logger.LogDebug("已存在权限数据, 跳过初始化");
            return;
        }

        List<Role> roles =
        [
            new Role { Id = 1, Name = "普通用户", Description = "默认用户", IsDefaultUserRole = true },
            new Role { Id = 2, Name = "无限制投稿用户", Description = "默认用户", IsDefaultUserRole = true },
            new Role { Id = 3, Name = "投稿审核员" },
            new Role { Id = 4, Name = "群聊管理员" },
            new Role { Id = 5, Name = "机器人管理员" },
        ];

        await InsertToTableInt(_roleRepository, roles).ConfigureAwait(false);
    }


    /// <summary>
    /// 初始化角色字段
    /// </summary>
    /// <returns></returns>
    private async Task InitDefaultRoleClaims()
    {
        ClearTableInt<IRoleClaimRepository, RoleClaim>(_roleClaimRepository).ConfigureAwait(false).GetAwaiter().GetResult();

        if (await _roleClaimRepository.CountAsync().ConfigureAwait(false) > 0)
        {
            _logger.LogDebug("已存在权限数据, 跳过初始化");
            return;
        }

        List<RoleClaim> roleClaims = [
            new RoleClaim{ RoleId = 1, ClaimId = 1 },
            new RoleClaim{ RoleId = 1, ClaimId = 1 },
            new RoleClaim{ RoleId = 1, ClaimId = 1 },
            new RoleClaim{ RoleId = 1, ClaimId = 1 },
            new RoleClaim{ RoleId = 1, ClaimId = 1 },
            new RoleClaim{ RoleId = 1, ClaimId = 1 },
        ];

        await InsertToTableInt(_roleClaimRepository, roleClaims).ConfigureAwait(false);
    }
}
