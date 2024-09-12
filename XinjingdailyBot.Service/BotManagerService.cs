using System.Collections.ObjectModel;
using Telegram.Bot;
using XinjingdailyBot.Infrastructure.Attribute;

namespace XinjingdailyBot.Service;

[AppService(LifeTime.Singleton)]
public sealed class BotManagerService
{
    private Dictionary<int, ITelegramBotClient> telegremBots = [];

    public ReadOnlyCollection<ITelegramBotClient> GetBots => telegremBots.Values.ToList().AsReadOnly();

    public void AddBot(int botId, ITelegramBotClient bot)
    {
        telegremBots.Add(botId, bot);
    }
}
