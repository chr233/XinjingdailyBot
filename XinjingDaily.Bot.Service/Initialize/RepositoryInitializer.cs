using Microsoft.Extensions.Logging;
using XinjingDaily.Bot.Interface.Bot.Storage;
using XinjingDaily.Bot.Interface.Common;
using XinjingDaily.Bot.Interface.InitService;

namespace XinjingDaily.Bot.Service.InitService;

/// <summary>
/// 机器人初始化服务
/// </summary>
/// <param name="_logger"></param>
[RegisterScoped<IServiceInitializer>(Duplicate = DuplicateStrategy.Append, Registration = RegistrationStrategy.ImplementedInterfaces)]
public class RepositoryInitializer(
    ILogger<RepositoryInitializer> _logger,
    IGlobalInfoService _globalInfo,
    IUserService _userService,
    IChatService _chatService,
    IPostService _postService) : IServiceInitializer
{
    /// <inheritdoc/>
    public int Order => 20;

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        await _userService.LoadUserSetting().ConfigureAwait(false);

        await _postService.LoadTagCache().ConfigureAwait(false);

        await _chatService.LoadChannelCache().ConfigureAwait(false);
    }
}