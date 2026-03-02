using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using XinjingDaily.Bot.Infrastructure.Attribute;
using XinjingDaily.Bot.Interface.InitService;

namespace XinjingDaily.Bot.Service.InitService;

/// <summary>
/// 命令初始化服务
/// </summary>
[RegisterScoped<IServiceInitializer>(Duplicate = DuplicateStrategy.Append)]
public class CommandInitializer(
    ILogger<CommandInitializer> _logger) : IServiceInitializer
{
    /// <inheritdoc/>
    public int Order => 3;

    /// <inheritdoc/>
    public string Name => nameof(CommandInitializer);

    /// <inheritdoc/>
    public Task InitializeAsync()
    {
        ScanExistCommands();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 安装命令
    /// </summary>
    [RequiresUnreferencedCode("不兼容剪裁")]
    private void ScanExistCommands()
    {
        _logger.LogInformation("开始扫描并注册命令...");

        /// <summary>
        /// 扫描程序集中的所有命令
        /// </summary>
        /// <param name="assemblies">要扫描的程序集</param>
        /// <returns>扫描到的命令列表</returns>
    public static List<object> DiscoverCommands()
    {
        List<CommandDescriptor>? commandList = new List<CommandDescriptor>();

        if (assemblies == null || assemblies.Length == 0)
        {
            // 默认扫描当前程序集
            assemblies = new[] { Assembly.GetExecutingAssembly() };
        }

        // 遍历所有程序集
        foreach (var assembly in assemblies)
        {
            // 遍历所有类型
            foreach (var type in assembly.GetTypes())
            {
                // 跳过抽象类和接口
                if (type.IsAbstract || type.IsInterface) continue;

                // 遍历类型中的所有方法
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    // 获取方法上的所有TextCmdAttribute
                    var cmdAttributes = method.GetCustomAttributes<TextCmdAttribute>(inherit: false);
                    foreach (var attr in cmdAttributes)
                    {
                        // 构建命令描述符
                        commandList.Add(new CommandDescriptor {
                            Keyword = attr.Keyword,
                            ChatType = attr.ChatType,
                            RequiredRight = attr.RequiredRight,
                            HandlerMethod = method,
                            HandlerType = type
                        });
                    }
                }
            }
        }

        return commandList;
    }

}