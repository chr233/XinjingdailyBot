namespace XinjingdailyBot.Interface.Bot.Common;

/// <summary>
/// 机器人接收服务
/// </summary>
public interface IReceiverService
{
    /// <summary>
    /// 机器人接收服务
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    Task ReceiveAsync(CancellationToken stoppingToken);
}
