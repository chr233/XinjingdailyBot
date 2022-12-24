namespace XinjingdailyBot.Interface.Bot
{
    public interface IReceiverService
    {
        Task ReceiveAsync(CancellationToken stoppingToken);
    }
}
