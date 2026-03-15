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
            new Claim { Id = 1, Name = "基础命令", Description = "", Value = "base:cmd" },
            new Claim { Id = 4, Name = "设置命令", Description = "", Value = "user:setting:cmd" },
            new Claim { Id = 3, Name = "3级命令", Description = "", Value = "adv-user:cmd" },
            new Claim { Id = 3, Name = "4级命令", Description = "", Value = "l4:command" },
            new Claim { Id = 3, Name = "5级命令", Description = "", Value = "l5:command" },
            new Claim { Id = 3, Name = "一般功能命令", Description = "", Value = "l2:command" },
            new Claim { Id = 4, Name = "群管功能命令", Description = "", Value = "group-admin:command" },
            new Claim { Id = 5, Name = "投稿功能命令", Description = "", Value = "group-admin:command" },
            new Claim { Id = 6, Name = "功能命令", Description = "", Value = "function:command" },
            new Claim { Id = 7, Name = "创建投稿", Description = "", Value = "post:create" },
            new Claim { Id = 8, Name = "执行审核命令", Description = "", Value = "review:command" },
            new Claim { Id = 9, Name = "执行审核命令", Description = "", Value = "review:command" },
            new Claim { Id = 10, Name = "执行审核命令", Description = "", Value = "review:command" },
            new Claim { Id = 11, Name = "用户管理", Description = "", Value = "UserManagement" },
            new Claim { Id = 12, Name = "角色管理", Description = "", Value = "RoleManagement" },

            new Claim { Id = 10, Name = "查询自己的信息", Description = "", Value = "query:self" },
            new Claim { Id = 11, Name = "查询所有人的信息", Description = "", Value = "query:all" },
            new Claim { Id = 100, Name = "自己审核", Description = "", Value = "RoleManagement" },
        ];

        claims.ForEach(static x => x.Value = x.Value?.ToUpperInvariant());

        DetectDuplicateClaims(claims);

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

    private void DetectDuplicateClaims(List<Claim> claims)
    {
        var duplicateClaims = claims
            .GroupBy(c => c.Id)            // 按Id分组
            .Where(g => g.Count() > 1) // 筛选出数量大于1的组（即重复Id）
            .Select(g => (g.Key, g.ToList()))
            .ToList();

        // 输出结果
        if (duplicateClaims.Count != 0)
        {
            Console.WriteLine("检测到重复的Claim Id：");
            Console.WriteLine("--------------------------------");
            foreach (var (key, items) in duplicateClaims)
            {
                Console.WriteLine($"重复Id：{key}，重复次数：{items.Count}");
                Console.WriteLine("对应的Claim元素：");
                foreach (var claim in items)
                {
                    Console.WriteLine($"  - Id:{claim.Id}, Name:{claim.Name}, Value:{claim.Value}");
                }
                Console.WriteLine("--------------------------------");
            }
            Environment.Exit(0);
        }
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

#if DEBUG
    private static async Task ClearTableInt<TRepository, TEntity>(TRepository repo)
        where TRepository : IRepository.Base.IRepositoryInt<TEntity>
        where TEntity : class, new()
    {
        await ClearTable<TRepository, TEntity, int>(repo).ConfigureAwait(false);
    }

    private static async Task ClearTable<TRepository, TEntity, TKey>(TRepository repo)
        where TRepository : IRepository.Base.IRepository<TEntity, TKey>
        where TEntity : class, new()
    {
        var entries = await repo.QueryAllAsync().ConfigureAwait(false);
        await repo.DeleteAsync(entries).ConfigureAwait(false);
    }
#endif
    #endregion

    /// <summary>
    /// 初始化权限字段
    /// </summary>
    /// <returns></returns>
    private async Task InitDefaultClaims(List<Claim> claims)
    {
        if (await _claimRepository.CountAsync().ConfigureAwait(false) > 0)
        {
            _logger.LogDebug("已存在权限数据, 跳过初始化");
#if RELEASE
            return;
#else
            ClearTableInt<IClaimRepository, Claim>(_claimRepository).ConfigureAwait(false).GetAwaiter().GetResult();
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

        if (await _roleRepository.CountAsync().ConfigureAwait(false) > 0)
        {
            _logger.LogDebug("已存在权限数据, 跳过初始化");
#if RELEASE
            return;
#else
            ClearTableInt<IRoleRepository, Role>(_roleRepository).ConfigureAwait(false).GetAwaiter().GetResult();
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

        if (await _roleClaimRepository.CountAsync().ConfigureAwait(false) > 0)
        {
            _logger.LogDebug("已存在权限数据, 跳过初始化");
#if RELEASE
            return;
#else
            ClearTableInt<IRoleClaimRepository, RoleClaim>(_roleClaimRepository).ConfigureAwait(false).GetAwaiter().GetResult();
#endif
        }

        await InsertToTableInt(_roleClaimRepository, roleClaims).ConfigureAwait(false);
    }


}
