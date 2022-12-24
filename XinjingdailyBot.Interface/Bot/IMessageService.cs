namespace XinjingdailyBot.Interface.Bot
{
    public interface IMessageService
    {
        Task HandleMessageAsync(CancellationToken stoppingToken);
    }
}
