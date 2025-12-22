using Telegram.Bot.Types;

namespace XinjingdailyBot.Service.Helper;

public interface IImageHelperService
{
    Task<string?> FuzzyImageCheck(Message message);
}