using Telegram.Bot;
using Telegram.Bot.Types;

namespace XinjingdailyBot.Interface.Bot.Common;

/// <summary>
/// 机器人消息更新服务
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// 处理Polling错误
    /// </summary>
    /// <param name="_"></param>
    /// <param name="exception"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandlePollingErrorAsync(ITelegramBotClient _, Exception exception, CancellationToken cancellationToken);
    /// <summary>
    /// 处理更新
    /// </summary>
    /// <param name="_"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken);
}
