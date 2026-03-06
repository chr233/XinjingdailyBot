using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingDaily.Bot.Entry.Entries.Command;
using XinjingDaily.Bot.Infrastructure.Attribute;
using XinjingDaily.Bot.Interface.Bot;

namespace XinjingDaily.Bot.Command.Common;

[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class NormalCommands(
    ITelegramBotService _botClient, ITelegramBotClient _bc)
{
    [TextCommand("CANCEL", "取消当前操作")]
    public async Task CancelCommand(CommandContext? context)
    {
       
    }
}
