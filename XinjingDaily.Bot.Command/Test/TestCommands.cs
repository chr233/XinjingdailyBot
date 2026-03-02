using XinjingDaily.Bot.Infrastructure.Attribute;

namespace XinjingDaily.Bot.Command.Test;

[RegisterScoped]
public class TestCommands
{
    [TextCmd("TEST", "测试指令")]
    public async Task TestCommand()
    {
        await Task.CompletedTask;
    }
}
