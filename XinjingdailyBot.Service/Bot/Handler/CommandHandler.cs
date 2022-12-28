using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Infrastructure.Model;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler;


[AppService(ServiceType = typeof(ICommandHandler), ServiceLifetime = LifeTime.Singleton)]
public class CommandHandler : ICommandHandler
{
    private readonly ILogger<CommandHandler> _logger;
    private readonly IChannelService _channelService;
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceScope _serviceScope;
    private readonly ICmdRecordService _cmdRecordService;
    private readonly OptionsSetting _optionsSetting;

    public CommandHandler(
        ILogger<CommandHandler> logger,
        IChannelService channelService,
        IServiceProvider serviceProvider,
        ITelegramBotClient botClient,
        ICmdRecordService cmdRecordService,
        IOptions<OptionsSetting> options)
    {
        _logger = logger;
        _channelService = channelService;
        _serviceScope = serviceProvider.CreateScope();
        _botClient = botClient;
        _cmdRecordService = cmdRecordService;
        _optionsSetting = options.Value;
    }

    /// <summary>
    /// 指令方法名映射
    /// </summary>
    private readonly Dictionary<Type, Dictionary<string, AssemblyMethod>> _commandClass = new();
    /// <summary>
    /// 指令别名
    /// </summary>
    private readonly Dictionary<Type, Dictionary<string, string>> _commandAlias = new();

    /// <summary>
    /// Query指令方法名映射
    /// </summary>
    private readonly Dictionary<Type, Dictionary<string, AssemblyMethod>> _queryCommandClass = new();
    /// <summary>
    /// Query指令别名
    /// </summary>
    private readonly Dictionary<Type, Dictionary<string, string>> _queryCommandAlias = new();

    /// <summary>
    /// 注册命令
    /// </summary>
    public void InstallCommands()
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
        Dictionary<string, AssemblyMethod> queryCommands = new();
        Dictionary<string, string> queryAlias = new();

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
                var command = queryAttribute.Command.ToUpperInvariant();
                var alias = queryAttribute.Alias?.ToUpperInvariant();
                var validUser = queryAttribute.ValidUser;
                var rights = queryAttribute.Rights;
                queryCommands.Add(command, new(method, validUser, rights));

                //添加别名
                if (!string.IsNullOrEmpty(alias))
                {
                    var splitedAlias = alias.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var split in splitedAlias)
                    {
                        queryAlias.Add(split, command);
                    }
                }
            }
        }

        if (commands.Count > 0)
        {
            _commandClass.Add(type, commands);
            _commandAlias.Add(type, commandAlias);
        }

        if (queryCommands.Count > 0)
        {
            _queryCommandClass.Add(type, queryCommands);
            _queryCommandAlias.Add(type, queryAlias);
        }
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="message"></param>
    /// <returns></returns>
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
        bool IsAtMe = false;
        int index = cmd.IndexOf('@');
        if (inGroup && index > -1)
        {
            string botName = cmd[(index + 1)..];
            if (botName.Equals(_channelService.BotUser.Username, StringComparison.OrdinalIgnoreCase))
            {
                cmd = cmd[..index];
                IsAtMe = true;
            }
            else
            {
                return;
            }
        }

        bool handled = false;
        string? errorMsg = null;
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
                try
                {
                    await CallCommandAsync(dbUser, message, type, method);
                }
                catch (Exception ex) //无法捕获 TODO
                {
                    errorMsg = $"{ex.GetType} {ex.Message}";

                    await _botClient.SendCommandReply(_optionsSetting.Debug ? errorMsg : "遇到内部错误", message);
                }
                handled = true;
                break;
            }
        }

        await _cmdRecordService.AddCmdRecord(message, dbUser, handled, false, errorMsg);

        if (!handled && ((inGroup && IsAtMe) || (!inGroup)))
        {
            await _botClient.SendCommandReply("未知的命令", message);
        }
    }

    /// <summary>
    /// 调用特定命令
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="message"></param>
    /// <param name="type"></param>
    /// <param name="assemblyMethod"></param>
    /// <returns></returns>
    private async Task CallCommandAsync(Users dbUser, Message message, Type type, AssemblyMethod assemblyMethod)
    {
        //权限检查
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
                case "String[]":
                    string[] args = message.Text!.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
                    methodParameters.Add(args[1..]);
                    break;

                default:
                    _logger.LogDebug("{paramName}", parameter.ParameterType.Name);
                    break;
            }
        }
        //调用方法
        method.Invoke(service, methodParameters.ToArray());
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public async Task OnQueryCommandReceived(Users dbUser, CallbackQuery query)
    {
        Message? message = query.Message;
        if (message == null)
        {
            await _botClient.AutoReplyAsync("消息不存在", query);
            return;
        }

        if (string.IsNullOrEmpty(query.Data))
        {
            await _botClient.RemoveMessageReplyMarkupAsync(message);
            return;
        }

        //切分命令参数
        string[] args = query.Data!.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
        string cmd = args.First();

        bool handled = false;
        string? errorMsg = null;
        //寻找注册的命令处理器
        foreach (var type in _queryCommandClass.Keys)
        {
            cmd = cmd.ToUpperInvariant();

            var allAlias = _queryCommandAlias[type];
            if (allAlias.TryGetValue(cmd, out var alias))
            {
                cmd = alias;
            }

            var allMethods = _queryCommandClass[type];
            if (allMethods.TryGetValue(cmd, out var method))
            {
                if (method.ValidUser)
                {
                    if (args.Length < 2 || !long.TryParse(args[0], out long userID))
                    {
                        await _botClient.AutoReplyAsync("Payload 非法", query);
                        await _botClient.RemoveMessageReplyMarkupAsync(message);
                        break;
                    }

                    //判断消息发起人是不是同一个
                    if (dbUser.UserID != userID)
                    {
                        await _botClient.AutoReplyAsync("这不是你的消息, 请不要瞎点", query);
                        break;
                    }
                }

                try
                {
                    await CallQueryCommandAsync(dbUser, query, type, method);
                }
                catch (Exception ex) //无法捕获 TODO
                {
                    errorMsg = $"{ex.GetType} {ex.Message}";

                    await _botClient.AutoReplyAsync(_optionsSetting.Debug ? errorMsg : "遇到内部错误", query);
                }
                handled = true;
                break;
            }
        }

        await _cmdRecordService.AddCmdRecord(message, dbUser, handled, true, errorMsg);

        if (!handled)
        {
            await _botClient.AutoReplyAsync("未知的命令", query);
        }
    }

    /// <summary>
    /// 调用特定命令
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="query"></param>
    /// <param name="type"></param>
    /// <param name="assemblyMethod"></param>
    /// <returns></returns>
    private async Task CallQueryCommandAsync(Users dbUser, CallbackQuery query, Type type, AssemblyMethod assemblyMethod)
    {
        //权限检查
        if (!dbUser.Right.HasFlag(assemblyMethod.Rights))
        {
            await _botClient.AutoReplyAsync("没有权限这么做", query);
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
                case nameof(CallbackQuery):
                    methodParameters.Add(query);
                    break;
                case "String[]":
                    string[] args = query.Data!.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
                    methodParameters.Add(args[1..]);
                    break;

                default:
                    _logger.LogDebug("{paramName}", parameter.ParameterType.Name);
                    break;
            }
        }
        //调用方法
        method.Invoke(service, methodParameters.ToArray());
    }
}
