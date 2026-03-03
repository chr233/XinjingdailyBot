using XinjingDaily.Bot.Infrastructure.Attribute;
using XinjingDaily.Bot.Infrastructure.Enums;

namespace XinjingDaily.Bot.Command.Test;

[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class TestCommands
{
    [Permission(ECommandScope.Group, "test")]
    [TextCommand("TEST", "测试指令")]
    public async Task TestCommand()
    {
        await Task.CompletedTask;
    }

    [Permission(ECommandScope.Group, "test")]
    [QueryCommand("TTTT")]
    public async Task TestQCommand()
    {
        await Task.CompletedTask;
    }
}
