namespace XinjingDaily.Bot.Interface.InitService;

public interface IInitializeService
{
    int Order { get; } 
    Task<bool> InitializeAsync(CancellationToken cancellationToken = default);
}
