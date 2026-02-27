using System.Collections.ObjectModel;
using Telegram.Bot;

namespace XinjingDaily.Bot.Service.Common;

[RegisterSingleton]
public sealed class BotManagerService
{
    private Dictionary<int, ITelegramBotClient> telegremBots = [];

    public ReadOnlyCollection<ITelegramBotClient> GetBots => telegremBots.Values.ToList().AsReadOnly();

    public void AddBot(int botId, ITelegramBotClient bot)
    {
        telegremBots.Add(botId, bot);
    }
}
