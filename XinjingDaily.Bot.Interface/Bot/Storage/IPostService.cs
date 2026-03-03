namespace XinjingDaily.Bot.Interface.Bot.Storage;

public interface IPostService
{
    Task<int> LoadTagCache();
}