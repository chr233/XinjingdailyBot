using Telegram.Bot.Types;
using XinjingDaily.Bot.Interface.Common;

namespace XinjingDaily.Bot.Service.Common;

[RegisterSingleton]
public sealed class GlobalInfoService : IGlobalInfoService
{
    public User BotUser { get; set; }


}
