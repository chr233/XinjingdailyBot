using Telegram.Bot;
using Telegram.Bot.Types;

namespace XinjingDaily.Bot.Interface.Bot.System;

public interface IUpdateService
{
    Task HandlePollingErrorAsync(ITelegramBotClient _, Exception exception, CancellationToken cancellationToken);
    Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken);
}