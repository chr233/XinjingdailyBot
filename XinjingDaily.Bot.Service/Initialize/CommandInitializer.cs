using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using XinjingDaily.Bot.Infrastructure.Attribute;
using XinjingDaily.Bot.Interface.Bot.Handler;
using XinjingDaily.Bot.Interface.InitService;

namespace XinjingDaily.Bot.Service.InitService;

/// <summary>
/// 命令初始化服务
/// </summary>
[RegisterScoped<IServiceInitializer>(Duplicate = DuplicateStrategy.Append)]
public class CommandInitializer(
    ILogger<CommandInitializer> _logger,
    ICommandHandler _commandHandler) : IServiceInitializer
{
    /// <inheritdoc/>
    public int Order => 3;

    /// <inheritdoc/>
    public string Name => nameof(CommandInitializer);

    /// <inheritdoc/>
    public Task InitializeAsync()
    {
        ScanExistCommands("XinjingDaily.Bot.Entry");
        return Task.CompletedTask;
    }

    /// <summary>
    /// 扫描程序集中的所有命令
    /// </summary>
    [RequiresUnreferencedCode("不兼容剪裁")]
    private void ScanExistCommands(string packageName)
    {
        var assembly = Assembly.Load(packageName);

        // 遍历所有类型
        foreach (var type in assembly.GetTypes())
        {
            // 跳过抽象类和接口
            if (type.IsAbstract || type.IsInterface) continue;

            // 遍历类型中的所有方法
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var textAttribute = method.GetCustomAttributes<TextCmdAttribute>(inherit: false);
                foreach (var attr in textAttribute)
                {
                    _commandHandler.RegisterTextCommand(type, method, attr);
                }
            }
        }
    }

}