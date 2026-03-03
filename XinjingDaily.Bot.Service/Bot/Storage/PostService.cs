using XinjingDaily.Bot.Interface.Bot.Storage;
using XinjingDaily.Bot.IRepository.Post;

namespace XinjingDaily.Bot.Service.Storage;

[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public sealed class PostService(
    IPostTagRepository _postTagRepository) : IPostService
{
    private readonly List<string> _tag = [];

    public async Task<int> LoadTagCache()
    {
        await _postTagRepository.QueryAllAsync().ConfigureAwait(false);

        return _tag.Count;
    }
}
