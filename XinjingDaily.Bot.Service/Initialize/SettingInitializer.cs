using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XinjingDaily.Bot.Entry.Columns;
using XinjingDaily.Bot.Entry.Entries.Users.Rbac;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Infrastructure.Strings;
using XinjingDaily.Bot.Infrastructure.Utils;
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
            new Claim(1, "公共命令", Permissions.CommonCommand),
            new Claim(2, "查询命令", Permissions.QueryCommand),
            new Claim(3, "4级命令", "l4:command"),
            new Claim(4, "5级命令", "l5:command"),
            new Claim(5, "一般功能命令", "l2:command"),
            new Claim(6, "群管功能命令", "group-admin:command"),
            new Claim(7, "投稿功能命令", "group-admin:command"),
            new Claim(8, "功能命令", "function:command"),
            new Claim(9, "创建投稿", "post:create"),
            new Claim(118, "执行审核命令", "review:command"),
            new Claim(19, "执行审核命令", "review:command"),
            new Claim(10, "执行审核命令", "review:command"),
            new Claim(11, "用户管理", "UserManagement"),
            new Claim(12, "角色管理", "RoleManagement"),
            new Claim(110, "查询自己的信息", "query:self"),
            new Claim(111, "查询所有人的信息", "query:all"),
            new Claim(21, "个人设置命令", "command:self:setting") ,
            new Claim(100, "自己审核", "RoleManagement"),
        ];

        claims.ForEach(static x => x.Value = x.Value?.ToUpperInvariant());

        DetectDuplicateClaims(claims);

        List<RoleClaimDefinition> roleDefinitions = [
            new RoleClaimDefinition( 1,"受限用户", "封禁用户", [1] ,true) ,
            new RoleClaimDefinition( 2,"初级用户", "默认角色, 未投稿过的用户", [1] ,true) ,
            new RoleClaimDefinition( 3,"普通用户", "投稿过的用户", [2,3,4] ),
            new RoleClaimDefinition( 4,"高级投稿用户", "无视投稿数量限制", [6,8,7] ),
            new RoleClaimDefinition( 5,"群聊管理员", "允许使用群管理命令", [3] ),
            new RoleClaimDefinition( 6,"投稿审核员", "允许使用投稿审核功能", [6] ),
            new RoleClaimDefinition( 7,"频道管理员", "允许设置发布频道", [4] ),
            new RoleClaimDefinition( 8,"机器人管理员", "允许修改机器人设置", [6] ),
            new RoleClaimDefinition( 9,"超级管理员", "最高的权限", [5] ),
        ];

        DetectMissingClaims(claims, roleDefinitions);

        var roles = roleDefinitions
            .Select(static x => new Role(x.Id, x.RoleName, x.RoleDescriptions, x.IsDefaultUserRole))
            .ToList();

        var roleClaims = roleDefinitions
            .SelectMany(static x => (x.ClaimKeys ?? []).Select(claimId => new RoleClaim(x.Id, claimId)))
            .Distinct()
            .ToList();

        await InitDefaultClaims(claims).ConfigureAwait(false);
        await InitDefaultRoles(roles).ConfigureAwait(false);
        await InitDefaultRoleClaims(roleClaims).ConfigureAwait(false);
    }

    private static void Terminal()
    {

    }

    private void DetectDuplicateClaims(List<Claim> claims)
    {
        var duplicateClaims = claims
            .GroupBy(c => c.Id)                     // 按Id分组
            .Where(g => g.Count() > 1)  // 筛选出数量大于1的组（即重复Id）
            .Select(g => (g.Key, g.ToList()))
            .ToList();

        // 输出结果
        if (duplicateClaims.Count != 0)
        {
            _logger.LogWarning("检测到重复的 Claim Id:");
            _logger.LogWarning(Langs.Line2);
            foreach (var (key, items) in duplicateClaims)
            {
                _logger.LogWarning("重复 Id：{key}，重复次数：{count}", key, items.Count);
                _logger.LogWarning("对应的 Claim 元素：");
                foreach (var claim in items)
                {
                    _logger.LogWarning("  - Id:{id}, Name:{name}, Value:{value}", claim.Id, claim.Name, claim.Value);
                }
                _logger.LogWarning(Langs.Line2);
            }

            SystemUtils.Shutdown();
        }
    }

    private void DetectMissingClaims(List<Claim> claims, List<RoleClaimDefinition> roleDefinitions)
    {
        var claimIds = claims.Select(c => c.Id).ToHashSet();

        var invalidRoleDetails = roleDefinitions
            .Where(static rd => rd.ClaimKeys != null)
            .Select(rd => new {
                Role = rd,
                InvalidClaimIds = rd.ClaimKeys?.Where(claimId => !claimIds.Contains(claimId)).ToList()
            })
            .Where(static x => x.InvalidClaimIds != null && x.InvalidClaimIds.Count != 0)
            .ToList();

        if (invalidRoleDetails.Count > 0)
        {
            foreach (var item in invalidRoleDetails)
            {
                foreach (var claim in item.InvalidClaimIds!)
                {
                    if (!claimIds.Contains(claim))
                    {
                        _logger.LogWarning("角色 {id} {roleName} 引用了不存在的 Claim Id {claimId}", item.Role.Id, item.Role.RoleName, claim);
                    }
                }
            }
            SystemUtils.Shutdown();
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
            if (entity is ICreateAt ca)
            {
                ca.CreateAt = create;
            }
            if (entity is IModifyAt ma)
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
