using Telegram.Bot.Types;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.Infrastructure.Attribute;
using XinjingDaily.Bot.Infrastructure.Enums;
using XinjingDaily.Bot.Interface.Bot;

namespace XinjingDaily.Bot.Command.Test;

[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class TestQueryCommands(
    ITelegramBotService _botClient)
{
    [QueryCommand("A")]
    public async Task ACommand(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }

    [Permission(ECommandScope.Private, null)]
    [QueryCommand("P")]
    public async Task PCommand(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }

    [Permission(ECommandScope.Group, null)]
    [QueryCommand("G")]
    public async Task GCommand(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }

    [Permission(ECommandScope.All, null)]
    [QueryCommand("TA")]
    public async Task TestCommand(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }

    [Permission(ECommandScope.Private, "test:1")]
    [Permission(ECommandScope.Group, "test:2")]
    [QueryCommand("TP")]
    public async Task Test1Command(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }

    [Permission(ECommandScope.All, "test:3")]
    [QueryCommand("TT")]
    public async Task Test2Command(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }
}
