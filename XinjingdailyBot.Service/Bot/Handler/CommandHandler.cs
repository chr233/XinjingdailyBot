using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Infrastructure.Model;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler;


[AppService(ServiceType = typeof(ICommandHandler), ServiceLifetime = LifeTime.Singleton)]
public class CommandHandler : ICommandHandler
{
    private readonly ILogger<CommandHandler> _logger;
    private readonly IChannelService _channelService;
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceScope _serviceScope;

    public CommandHandler(
        ILogger<CommandHandler> logger,
        IChannelService channelService,
        IServiceProvider serviceProvider,
        ITelegramBotClient botClient)
    {
        _logger = logger;
        _channelService = channelService;
        _serviceScope = serviceProvider.CreateScope();
        _botClient = botClient;
    }

    /// <summary>
    /// 类映射的方法
    /// </summary>
    private readonly Dictionary<Type, Dictionary<string, AssemblyMethod>> _commandClass = new();
    /// <summary>
    /// 命令别名
    /// </summary>
    private readonly Dictionary<Type, Dictionary<string, string>> _commandAlias = new();

    //private readonly Dictionary<


    public async Task OnCommandReceived(Users dbUser, Message message)
    {
        if (string.IsNullOrEmpty(message.Text))
        {
            return;
        }

        //切分命令参数
        string[] args = message.Text!.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
        string cmd = args.First()[1..];
        bool inGroup = message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup;

        //判断是不是艾特机器人的命令
        if (inGroup && cmd.Contains('@'))
        {
            int index = cmd.IndexOf('@');
            string botName = cmd[(index + 1)..];
            if (botName.Equals(_channelService.BotUser.Username, StringComparison.OrdinalIgnoreCase))
            {
                cmd = cmd[..index];
            }
            else
            {
                return;
            }
        }

        bool handled = false;
        //寻找注册的命令处理器
        foreach (var type in _commandClass.Keys)
        {
            cmd = cmd.ToUpperInvariant();

            var allAlias = _commandAlias[type];
            if (allAlias.TryGetValue(cmd, out var alias))
            {
                cmd = alias;
            }

            var allMethods = _commandClass[type];
            if (allMethods.TryGetValue(cmd, out var method))
            {
                await CallCommand(dbUser, message, type, method);
                handled=true;
                break;
            }
        }

        if (!handled)
        {
            await _botClient.SendCommandReply("未知的命令", message);
        }
    }

    private async Task CallCommand(Users dbUser, Message message, Type type, AssemblyMethod assemblyMethod)
    {
        if (!dbUser.Right.HasFlag(assemblyMethod.Rights))
        {
            await _botClient.SendCommandReply("没有权限这么做", message);
            return;
        }

        //获取服务
        var service = _serviceScope.ServiceProvider.GetRequiredService(type);
        var method = assemblyMethod.Method;
        var methodParameters = new List<object>() { };
        //组装函数的入参
        foreach (var parameter in method.GetParameters())
        {
            switch (parameter.ParameterType.Name)
            {
                case nameof(Users):
                    methodParameters.Add(dbUser);
                    break;
                case nameof(Message):
                    methodParameters.Add(message);
                    break;
                default:
                    break;
            }
        }
        //调用方法
        method.Invoke(service, methodParameters.ToArray());
    }

    public async Task InitCommands()
    {

        //获取所有服务方法
        var assembly = Assembly.Load("XinjingdailyBot.Command");
        foreach (var type in assembly.GetTypes())
        {
            RegisterCommands(type);
        }
    }

    /// <summary>
    /// 注册命令
    /// </summary>
    /// <param name="type"></param>
    private void RegisterCommands(Type type)
    {
        Dictionary<string, AssemblyMethod> commands = new();
        Dictionary<string, string> commandAlias = new();

        foreach (var method in type.GetMethods())
        {
            var textAttribute = method.GetCustomAttribute<TextCmdAttribute>();

            //注册文字命令
            if (textAttribute != null)
            {
                var command = textAttribute.Command.ToUpperInvariant();
                var alias = textAttribute.Alias?.ToUpperInvariant();
                var description = textAttribute.Description;
                var rights = textAttribute.Rights;
                commands.Add(command, new(method, description, rights));

                //添加别名
                if (!string.IsNullOrEmpty(alias))
                {
                    var splitedAlias = alias.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var split in splitedAlias)
                    {
                        commandAlias.Add(split, command);
                    }
                }
            }

            var queryAttribute = method.GetCustomAttribute<QueryCmdAttribute>();

            //注册Query命令
            if (queryAttribute != null)
            {
                //todo
            }
        }

        if (commands.Count > 0)
        {
            _commandClass.Add(type, commands);
            _commandAlias.Add(type, commandAlias);
        }
    }
}
