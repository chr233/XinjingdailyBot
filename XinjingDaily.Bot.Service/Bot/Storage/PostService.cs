using Microsoft.Extensions.Logging;
using XinjingDaily.Bot.Interface.Bot.Storage;
using XinjingDaily.Bot.IRepository.Post;

namespace XinjingDaily.Bot.Service.Storage;

[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public sealed class PostService(
    ILogger<PostService> _logger,
    IPostTagRepository _postTagRepository) : IPostService
{
    private readonly List<string> _tag = [];

    public async Task LoadTagCache()
    {
        await _postTagRepository.QueryAllAsync().ConfigureAwait(false);

        _logger.LogInformation("读取了 {count} 个投稿标签", _tag.Count);


    }
}
