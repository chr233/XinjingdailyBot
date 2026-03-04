using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingDaily.Bot.Infrastructure.Attribute;
using XinjingDaily.Bot.Infrastructure.Enums;
using XinjingDaily.Bot.Interface.Bot;

namespace XinjingDaily.Bot.Command.Common;

[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class NormalCommands(
    ITelegramBotService _botClient, ITelegramBotClient _bc)
{

    [Permission(ECommandScope.Group, "test:1")]
    [QueryCommand("CLEARCOMMAND")]
    public async Task TestQCommand()
    {
        await _bc.DeleteMyCommands(BotCommandScope.AllGroupChats());
    }
}
