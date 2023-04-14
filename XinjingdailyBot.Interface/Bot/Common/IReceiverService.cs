namespace XinjingdailyBot.Interface.Bot.Common
{
    public interface IReceiverService
    {
        /// <summary>
        /// 机器人接收服务
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        Task ReceiveAsync(CancellationToken stoppingToken);
    }
}
