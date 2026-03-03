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
        var tagCount = await _postService.LoadTagCache().ConfigureAwait(false);
        _logger.LogInformation("读取了 {count} 个投稿标签", tagCount);

        var channelCount = await _chatService.LoadChannelCache().ConfigureAwait(false);
        _logger.LogInformation("读取了 {count} 个投稿频道", channelCount);

    }
}