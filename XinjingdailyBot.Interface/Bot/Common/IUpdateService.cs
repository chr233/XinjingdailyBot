using Telegram.Bot;
using Telegram.Bot.Types;

namespace XinjingdailyBot.Interface.Bot.Common;

/// <summary>
/// 机器人消息更新服务
/// </summary>
public interface IUpdateService
{
    Task HandlePollingErrorAsync(ITelegramBotClient _, Exception exception, CancellationToken cancellationToken);
    Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken);
}
