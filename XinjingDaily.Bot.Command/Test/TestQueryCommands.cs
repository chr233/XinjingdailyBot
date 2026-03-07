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
    [QueryCommand("A", "测试命令 - 默认权限")]
    public async Task ACommand(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }

    [Permission(ECommandScope.Private, null)]
    [QueryCommand("P", "测试指令 - 私聊命令")]
    public async Task PCommand(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }

    [Permission(ECommandScope.Group, null)]
    [QueryCommand("G", "测试指令 - 群组命令")]
    public async Task GCommand(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }

    [Permission(ECommandScope.All, null)]
    [QueryCommand("TA", "测试指令 - 全部场景")]
    public async Task TestCommand(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }

    [Permission(ECommandScope.Private, "test:1")]
    [Permission(ECommandScope.Group, "test:2")]
    [QueryCommand("TP", "测试指令 - 权限分级")]
    public async Task Test1Command(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }

    [Permission(ECommandScope.All, "test:3")]
    [QueryCommand("TT", "测试指令")]
    public async Task Test2Command(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }
}
