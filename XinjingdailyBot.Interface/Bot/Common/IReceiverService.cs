namespace XinjingdailyBot.Interface.Bot.Common
{
    public interface IReceiverService
    {
        Task ReceiveAsync(CancellationToken stoppingToken);
    }
}
