namespace XinjingdailyBot.Interface.Bot
{
    public interface ICallbackService
    {
        Task HandleCallbackAsync(CancellationToken stoppingToken);
    }
}
