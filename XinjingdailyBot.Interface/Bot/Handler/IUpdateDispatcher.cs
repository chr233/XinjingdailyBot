using Telegram.Bot;
using Telegram.Bot.Types;

namespace XinjingdailyBot.Interface.Bot.Handler
{
    public interface IUpdateDispatcher
    {
        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    }
}
