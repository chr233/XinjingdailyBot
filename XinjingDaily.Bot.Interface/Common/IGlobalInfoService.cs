using Telegram.Bot.Types;

namespace XinjingDaily.Bot.Interface.Common;

public interface IGlobalInfoService
{
    User BotUser { get; set; }
}