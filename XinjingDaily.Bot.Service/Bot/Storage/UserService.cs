using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Infrastructure.Extensions;
using XinjingDaily.Bot.Infrastructure.Strings;
using XinjingDaily.Bot.Interface.Bot.Storage;
using XinjingDaily.Bot.IRepository.History.User;
using XinjingDaily.Bot.IRepository.Redis;
using XinjingDaily.Bot.IRepository.User;
using XinjingDaily.Bot.IRepository.User.Rbac;

namespace XinjingDaily.Bot.Service.Storage;

[RegisterSingleton(Registration = RegistrationStrategy.ImplementedInterfaces)]
public sealed class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly AppSettings _options;
    private readonly IUserInfoRepository _userInfoRepository;
    private readonly IUserInfoHistoryRepository _userInfoHistoryRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRoleClaimRepository _roleClaimRepository;
    private readonly IUserClaimRepository _userClaimRepository;
    private readonly IClaimRepository _claimRepository;
    private readonly IRedisRepository _redisRepository;

    private readonly TimeSpan UpdatePeriod = TimeSpan.FromDays(14);

    private readonly HashSet<long> SuperAdminUserIds = [];
    private readonly HashSet<string> SuperAdminUserNames = [];

    public UserService(
        ILogger<UserService> logger,
        IOptions<AppSettings> options,
        IServiceScopeFactory _serviceScopeFactory)
    {
        _logger = logger;
        _options = options.Value;

        var scope = _serviceScopeFactory.CreateScope();
        _userInfoRepository = scope.ServiceProvider.GetRequiredService<IUserInfoRepository>();
        _userInfoHistoryRepository = scope.ServiceProvider.GetRequiredService<IUserInfoHistoryRepository>();
        _userRoleRepository = scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();
        _roleClaimRepository = scope.ServiceProvider.GetRequiredService<IRoleClaimRepository>();
        _userClaimRepository = scope.ServiceProvider.GetRequiredService<IUserClaimRepository>();
        _claimRepository = scope.ServiceProvider.GetRequiredService<IClaimRepository>();
        _redisRepository = scope.ServiceProvider.GetRequiredService<IRedisRepository>();
    }

    public async Task LoadUserSetting()
    {
        var admins = _options.System.SuperAdmins;
        if (admins != null && admins.Count > 0)
        {
            foreach (var admin in admins)
            {
                if (string.IsNullOrEmpty(admin))
                {
                    continue;
                }

                if (long.TryParse(admin, out var userId))
                {
                    SuperAdminUserIds.Add(userId);
                }
                else if (!string.IsNullOrEmpty(admin))
                {
                    if (admin.StartsWith('@'))
                    {
                        SuperAdminUserNames.Add(admin.TrimStart('@'));
                    }
                    else
                    {
                        SuperAdminUserNames.Add(admin);
                    }
                }
            }
        }

        _logger.LogInformation("读取了 {count} 个超级管理员", SuperAdminUserIds.Count + SuperAdminUserNames.Count);

    }

    public async Task<UserInfo?> QueryUserFromUpdate(Update update)
    {
        var msgChat = update.Type switch {
            UpdateType.ChannelPost => update.ChannelPost!.Chat,
            UpdateType.EditedChannelPost => update.EditedChannelPost!.Chat,
            UpdateType.Message => update.Message!.Chat,
            UpdateType.EditedMessage => update.EditedMessage!.Chat,
            UpdateType.ChatJoinRequest => update.ChatJoinRequest!.Chat,
            _ => null
        };

        var message = update.Type switch {
            UpdateType.Message => update.Message!,
            UpdateType.ChannelPost => update.ChannelPost!,
            _ => null,
        };

        if (update.Type == UpdateType.ChannelPost)
        {
            return await QueryUserFromChannelPost(update.ChannelPost!).ConfigureAwait(false);
        }
        else
        {
            var msgUser = update.Type switch {
                UpdateType.ChannelPost => update.ChannelPost!.From,
                UpdateType.EditedChannelPost => update.EditedChannelPost!.From,
                UpdateType.Message => update.Message!.From,
                UpdateType.EditedMessage => update.EditedMessage!.From,
                UpdateType.CallbackQuery => update.CallbackQuery!.From,
                UpdateType.InlineQuery => update.InlineQuery!.From,
                UpdateType.ChosenInlineResult => update.ChosenInlineResult!.From,
                UpdateType.ChatJoinRequest => update.ChatJoinRequest!.From,
                _ => null
            };

            return await QueryUserFromChat(msgUser, msgChat).ConfigureAwait(false);
        }
    }



    /// <summary>
    /// 根据ChannelPost Author获取用户
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task<UserInfo?> QueryUserFromChannelPost(Message message)
    {
        //if (message.Chat.Id != _channelService.AcceptChannel.Id)
        //{
        //    return null;
        //}

        string? author = message.AuthorSignature;
        if (string.IsNullOrEmpty(author))
        {
            return null;
        }

        _logger.LogWarning(author);

        //if (_channelUserIdCache.TryGetValue(author, out long userId))
        //{
        //    return await QueryUserByUserId(userId).ConfigureAwait(false);
        //}
        //else //缓存中没有该用户, 更新缓存
        //{
        //    var admins = await _botClient.GetChatAdministrators(message.Chat).ConfigureAwait(false);
        //    if (admins == null)
        //    {
        //        return null;
        //    }
        //    foreach (var admis in admins)
        //    {
        //        string name = admis.User.FullName();
        //        _channelUserIdCache[name] = admis.User.Id;
        //    }

        //    if (_channelUserIdCache.TryGetValue(author, out userId))
        //    {
        //        return await QueryUserByUserId(userId).ConfigureAwait(false);
        //    }
        //}
        return null;
    }

    /// <summary>
    /// 创建新用户
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task<UserInfo?> CreateNewUser(User user)
    {
        var userInfo = new UserInfo {
            TelegramId = user.Id,
            TelegramName = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsBot = user.IsBot,
            IsBan = false,
            CreateAt = DateTime.UtcNow,
            ModifyAt = DateTime.MinValue,
        };

        try
        {
            userInfo.Id = await _userInfoRepository.InsertAsync(userInfo).ConfigureAwait(false);

            if (_options.System.Debug)
            {
                _logger.LogDebug("创建用户 {user} 成功", userInfo);
            }

            return userInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建用户 {user} 失败", userInfo);
            return null;
        }
    }

    /// <summary>
    /// 从缓存获取用户权限列表
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    private async Task<HashSet<string>?> GetUserClaimsFromCache(int userId)
    {
        var cacheKey = $"{CacheKeys.UserPermission}:{userId}";
        return await _redisRepository.GetAsync<HashSet<string>>(cacheKey).ConfigureAwait(false);
    }

    /// <summary>
    /// 从缓存设置用户权限列表
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="claims"></param>
    /// <returns></returns>
    private async Task SetUserClaimsToCache(int userId, HashSet<string> claims)
    {
        var cacheKey = $"{CacheKeys.UserPermission}:{userId}";
        if (claims != null)
        {
            var expirySeconds = _options.Cache.UserPermissionTtl;
            await _redisRepository.SetAsync(cacheKey, claims, expirySeconds, When.Always).ConfigureAwait(false);
        }
        else
        {
            await _redisRepository.DeleteAsync(cacheKey).ConfigureAwait(false);
        }
    }

    private bool IsSuperAdmin(UserInfo userInfo)
    {
        return SuperAdminUserIds.Contains(userInfo.Id) ||
            (!string.IsNullOrEmpty(userInfo.TelegramName) && SuperAdminUserNames.Contains(userInfo.TelegramName));
    }

    public async Task<HashSet<string>> QueryUserClaims(UserInfo userInfo)
    {
        // 验证用户是否有效
        HashSet<string> claims = [];
        if (userInfo == null || userInfo.Id <= 0 || userInfo.IsBan)
        {
            return claims;
        }

        // 尝试从缓存加载权限
        var cachedClaims = await GetUserClaimsFromCache(userInfo.Id).ConfigureAwait(false);
        if (cachedClaims != null)
        {
            return cachedClaims;
        }

        if (IsSuperAdmin(userInfo))
        {
            var allClaims = await _claimRepository.QueryAllClaimsAsync().ConfigureAwait(false);

            if (allClaims != null)
            {
                foreach (var claim in allClaims)
                {
                    claims.Add(claim);
                }
            }
        }
        else
        {
            // 间接权限子查询：UserRole -> Role -> RoleClaim -> Claim
            var indirectKeys = await _userRoleRepository
                .QueryUserRoleClaimsAsync(userInfo)
                .ConfigureAwait(false);

            // 直接权限子查询：UserClaim -> Claim
            var directKeys = await _userClaimRepository
                    .QueryUserClaimsAsync(userInfo)
                    .ConfigureAwait(false);

            foreach (var key in directKeys)
            {
                claims.Add(key ?? "");
            }
            foreach (var key in indirectKeys)
            {
                claims.Add(key ?? "");
            }
        }

        await SetUserClaimsToCache(userInfo, claims).ConfigureAwait(false);

        return claims;
    }

    /// <summary>
    /// 根据MessageUser获取用户
    /// </summary>
    /// <param name="user"></param>
    /// <param name="chat"></param>
    /// <returns></returns>
    private async Task<UserInfo?> QueryUserFromChat(User? user, Chat? chat)
    {
        if (user == null)
        {
            return null;
        }

        bool isDebug = _options.System.Debug;

        if (user.Username == "GroupAnonymousBot")
        {
            if (isDebug)
            {
                if (chat != null)
                {
                    _logger.LogDebug("忽略群匿名用户 {chatProfile}", chat.ChatProfile());
                }
            }
            return null;
        }

        var userInfo = await _userInfoRepository.QueryByTelegramIdAsync(user.Id).ConfigureAwait(false);
        if (userInfo == null)
        {
            userInfo = await CreateNewUser(user).ConfigureAwait(false);

            if (userInfo == null)
            {
                return null;
            }
        }
        else
        {
            var nickChanged = userInfo.FirstName != user.FirstName || userInfo.LastName != user.LastName;
            var userNameChanged = userInfo.TelegramName != user.Username;

            //用户名不一致时
            if (nickChanged || userNameChanged)
            {
                userInfo.TelegramName = user.Username;
                userInfo.FirstName = user.FirstName;
                userInfo.LastName = user.LastName;

                await _userInfoHistoryRepository.CreateHistoryAsync(userInfo, nickChanged, userNameChanged).ConfigureAwait(false);
                await _userInfoRepository.UpdateTelegramNameAndNickNameAsync(userInfo).ConfigureAwait(false);
            }

            if (userInfo.IsBot != user.IsBot)
            {
                userInfo.IsBot = user.IsBot;
                await _userInfoRepository.UpdateIsBotAsync(userInfo).ConfigureAwait(false);
            }

            //超过设定时间也触发更新
            if (DateTime.UtcNow > userInfo.ModifyAt + UpdatePeriod)
            {
                await _userInfoRepository.UpdateModifyAsync(userInfo).ConfigureAwait(false);
            }
        }

        return userInfo;
    }
}
