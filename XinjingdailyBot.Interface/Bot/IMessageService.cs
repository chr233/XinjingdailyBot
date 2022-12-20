namespace XinjingdailyBot.Interface.Bot
{

    /// <summary>
    /// A marker interface for Update Receiver service
    /// </summary>
    public interface IMessageService
    {
        Task HandleMessageAsync(CancellationToken stoppingToken);
    }
}