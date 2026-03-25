using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingDaily.Bot.Data.Bot;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Infrastructure.Attribute;
using XinjingDaily.Bot.Infrastructure.Enums;
using XinjingDaily.Bot.Interface.Bot;
using XinjingDaily.Bot.Interface.Bot.Handler;
using XinjingDaily.Bot.Interface.Bot.Storage;
using XinjingDaily.Bot.Interface.Common;

namespace XinjingDaily.Bot.Service.Bot.Handler;

[RegisterSingleton(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class CommandHandler : ICommandHandler
{
    private readonly ILogger<CommandHandler> _logger;
    private readonly ITelegramBotService _botClient;
    private readonly IGlobalInfoService _globalInfo;

    private readonly AppSettings _optionsSetting;
    private readonly IServiceScope _serviceScope;
    private readonly IUserService _userService;

    /// <summary>
    /// 命令权限字典, key: (scope, command), value: permission
    /// </summary>
    private readonly Dictionary<(ECommandScope scope, string command), string?> _textCommandPermission = [];
    /// <summary>
    /// 命令定义字典, key: command, value: (classType, methodInfo, attribute)
    /// </summary>
    private readonly Dictionary<string, CommandDefinition<TextCommandAttribute>> _textCommandDefinitions = [];
    /// <summary>
    /// 命令权限字典, key: (scope, command), value: permission
    /// </summary>
    private readonly Dictionary<(ECommandScope scope, string command), string?> _queryCommandPermission = [];
    /// <summary>
    /// 命令定义字典, key: command, value: (classType, methodInfo, attribute)
    /// </summary>
    private readonly Dictionary<string, CommandDefinition<QueryCommandAttribute>> _queryCommandDefinitions = [];

    public CommandHandler(
        ILogger<CommandHandler> logger,
        IServiceProvider serviceProvider,
        ITelegramBotService botClient,
        IGlobalInfoService globalInfo,
        IOptions<AppSettings> options)
    {
        _logger = logger;
        _botClient = botClient;
        _globalInfo = globalInfo;
        _optionsSetting = options.Value;

        var scope = serviceProvider.CreateScope();
        _serviceScope = scope;
        _userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    }

    private static string[] SplitAlias(string alias)
    {
        return alias.ToUpperInvariant().Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static string[] SplitArgs(string args)
    {
        return args.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    #region 注册命令
    /// <inheritdoc />
    public void RegisterTextCommand(Type classType, MethodInfo methodInfo, TextCommandAttribute attribute)
    {
        RegisterTextCommand(classType, methodInfo, ECommandScope.All, null, attribute);
    }

    /// <inheritdoc />
    public void RegisterTextCommand(Type classType, MethodInfo methodInfo, PermissionAttribute permission, TextCommandAttribute attribute)
    {
        RegisterTextCommand(classType, methodInfo, permission.Scope, permission.Permission, attribute);
    }

    /// <inheritdoc />
    public void RegisterTextCommand(Type classType, MethodInfo methodInfo, ECommandScope scope, string? permission, TextCommandAttribute attribute)
    {
        var command = attribute.Command.ToUpperInvariant();
        permission = permission?.ToUpperInvariant();
        var definition = new CommandDefinition<TextCommandAttribute>(classType, methodInfo, attribute);

        _textCommandDefinitions.TryAdd(command, definition);
        if (!_textCommandPermission.TryAdd((scope, command), permission))
        {
            _logger.LogWarning("命令 {scope} - {command} - {permission} 已经存在, 请检查代码逻辑", scope, command, permission);
        }
        else
        {
            _logger.LogDebug("注册 {scope} - {command} - {permission}", scope, command, permission);
        }

        if (!string.IsNullOrEmpty(attribute.Alias))
        {
            var entries = SplitAlias(attribute.Alias);
            foreach (var entry in entries)
            {
                if (!_textCommandDefinitions.TryAdd(entry, definition))
                {
                    _logger.LogWarning("命令 {command} 的别名 {alias} 已经存在, 请检查代码逻辑", command, entry);
                }
                _textCommandPermission.TryAdd((scope, entry), permission);
            }
        }
    }

    public void RegisterQueryCommand(Type classType, MethodInfo methodInfo, QueryCommandAttribute attribute)
    {
        RegisterQueryCommand(classType, methodInfo, ECommandScope.All, null, attribute);
    }

    public void RegisterQueryCommand(Type classType, MethodInfo methodInfo, PermissionAttribute permission, QueryCommandAttribute attribute)
    {
        RegisterQueryCommand(classType, methodInfo, permission.Scope, permission.Permission, attribute);
    }

    public void RegisterQueryCommand(Type classType, MethodInfo methodInfo, ECommandScope scope, string? permission, QueryCommandAttribute attribute)
    {
        var command = attribute.Command.ToUpperInvariant();
        permission = permission?.ToUpperInvariant();
        var definition = new CommandDefinition<QueryCommandAttribute>(classType, methodInfo, attribute);

        _queryCommandDefinitions.TryAdd(command, definition);
        if (!_queryCommandPermission.TryAdd((scope, command), permission))
        {
            _logger.LogWarning("Q命令 {scope} - {command} - {permission} 已经存在, 请检查代码逻辑", scope, command, permission);
        }
        else
        {
            _logger.LogDebug("Q注册 {scope} - {command} - {permission}", scope, command, permission);
        }

        if (!string.IsNullOrEmpty(attribute.Alias))
        {
            var entries = SplitAlias(attribute.Alias);
            foreach (var entry in entries)
            {
                if (!_queryCommandDefinitions.TryAdd(command, definition))
                {
                    _logger.LogWarning("Q命令 {command} 的别名 {alias} 已经存在, 请检查代码逻辑", command, entry);
                }
                _queryCommandPermission.TryAdd((scope, entry), permission);
            }
        }
    }
    #endregion

    #region 验证权限
    /// <summary>
    /// 验证Text命令权限
    /// </summary>
    /// <param name="claims"></param>
    /// <param name="scope"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    private bool VerifyTextCommandPermission(HashSet<string>? claims, ECommandScope scope, string command)
    {
        if (_textCommandPermission.TryGetValue((scope, command), out var permission))
        {
            return permission == null || (claims != null && claims.Contains(permission));
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 验证Text命令权限
    /// </summary>
    /// <param name="claims"></param>
    /// <param name="chatType"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    private bool VerifyTextCommandPermission(HashSet<string>? claims, ChatType chatType, string command)
    {
        if (chatType is ChatType.Private)
        {
            if (VerifyTextCommandPermission(claims, ECommandScope.Private, command))
            {
                return true;
            }
        }
        else if (chatType is ChatType.Group or ChatType.Supergroup)
        {
            if (VerifyTextCommandPermission(claims, ECommandScope.Group, command))
            {
                return true;
            }
        }

        return VerifyTextCommandPermission(claims, ECommandScope.All, command);
    }

    /// <summary>
    /// 验证Query命令权限
    /// </summary>
    /// <param name="claims"></param>
    /// <param name="scope"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    private bool VerifyQueryCommandPermission(HashSet<string>? claims, ECommandScope scope, string command)
    {
        if (_queryCommandPermission.TryGetValue((scope, command), out var permission))
        {
            return permission == null || (claims != null && claims.Contains(permission));
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 验证Query命令权限
    /// </summary>
    /// <param name="claims"></param>
    /// <param name="chatType"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    private bool VerifyQueryCommandPermission(HashSet<string>? claims, ChatType chatType, string command)
    {
        if (chatType is ChatType.Private)
        {
            if (VerifyQueryCommandPermission(claims, ECommandScope.Private, command))
            {
                return true;
            }
        }
        else if (chatType is ChatType.Group or ChatType.Supergroup)
        {
            if (VerifyQueryCommandPermission(claims, ECommandScope.Group, command))
            {
                return true;
            }
        }

        return VerifyQueryCommandPermission(claims, ECommandScope.All, command);
    }
    #endregion

    /// <inheritdoc />
    public async Task OnCommandReceived(UserInfo userInfo, Message message)
    {
        if (string.IsNullOrEmpty(message.Text))
        {
            return;
        }

        //切分命令参数
        var args = SplitArgs(message.Text);
        var cmd = args.First().TrimStart('/').ToUpperInvariant();
        bool isInGroup = message.Chat.Type is ChatType.Group or ChatType.Supergroup;

        //判断是不是艾特机器人的命令
        bool isAtMe = false;
        int index = cmd.IndexOf('@');
        if (isInGroup && index > -1)
        {
            string botName = cmd[(index + 1)..];
            if (botName.Equals(_globalInfo.BotUser.Username, StringComparison.OrdinalIgnoreCase))
            {
                cmd = cmd[..index];
                isAtMe = true;
            }
            else
            {
                return;
            }
        }

        bool handled = false;
        string? errorMsg = null;

        //寻找注册的命令处理器
        if (_textCommandDefinitions.TryGetValue(cmd, out var definition))
        {
            var claims = await _userService.QueryUserClaims(userInfo).ConfigureAwait(false);

            // 根据调用环境获取需要的权限
            if (VerifyTextCommandPermission(claims, message.Chat.Type, cmd))
            {
                try
                {
                    await _botClient.SendChatAction(message, ChatAction.Typing).ConfigureAwait(false);

                    await CallCommandAsync(userInfo, message, definition.ClassType, definition.Method, args).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    errorMsg = $"{ex.GetType} {ex.Message}";
                    _logger.LogError(ex, "命令 {cmd} 执行出错", cmd);
                    await _botClient.SendCommandReply(_optionsSetting.System.Debug ? errorMsg : "遇到内部错误", message).ConfigureAwait(false);
                }
                handled = true;
            }
            else
            {
                if (isAtMe || !isInGroup)
                {
                    await _botClient.SendCommandReply("没有权限这么做", message).ConfigureAwait(false);
                }
            }
        }
        else
        {
            if (isAtMe || !isInGroup)
            {
                await _botClient.SendCommandReply("未知的命令", message).ConfigureAwait(false);
            }
        }

        //await _cmdRecordService.AddCmdRecord(message, userInfo, handled, false, errorMsg).ConfigureAwait(false);
    }

    /// <summary>
    /// 调用特定命令
    /// </summary>
    /// <param name="userInfo"></param>
    /// <param name="message"></param>
    /// <param name="type"></param>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private async Task CallCommandAsync(UserInfo userInfo, Message message, Type type, MethodInfo method, string[] args)
    {
        List<object> methodParameters = [];

        //获取服务
        var service = _serviceScope.ServiceProvider.GetRequiredService(type);

        //组装函数的入参
        foreach (var parameter in method.GetParameters())
        {
            var paramType = parameter.ParameterType;

            if (paramType == typeof(UserInfo))
            {
                methodParameters.Add(userInfo);
            }
            else if (paramType == typeof(Message))
            {
                methodParameters.Add(message);
            }
            else if (paramType == typeof(string[]))
            {
                var methodArgs = args.Length > 0 ? args[1..] : [];
                methodParameters.Add(methodArgs);
            }
            else if (paramType == typeof(List<string>))
            {
                var methodArgs = args.Length > 0 ? args[1..] : [];
                methodParameters.Add(methodArgs.ToList());
            }
            else
            {
                _logger.LogError("无效的参数类型 {paramName}", paramType.FullName);
            }
        }
        //调用方法
        if (method.Invoke(service, [.. methodParameters]) is Task task)
        {
            await task.ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task OnQueryCommandReceived(UserInfo userInfo, CallbackQuery query)
    {
        var message = query.Message;
        if (message == null)
        {
            await _botClient.AutoReply("消息不存在", query, true).ConfigureAwait(false);
            return;
        }

        if (string.IsNullOrEmpty(query.Data))
        {
            await _botClient.RemoveMessageReplyMarkup(message).ConfigureAwait(false);
            return;
        }

        //切分命令参数
        var args = SplitArgs(query.Data);
        var cmd = args.First().ToUpperInvariant();
        bool isInGroup = message.Chat.Type is ChatType.Group or ChatType.Supergroup;

        bool handled = false;
        string? errorMsg = null;

        //寻找注册的命令处理器
        if (_queryCommandDefinitions.TryGetValue(cmd, out var definition))
        {
            var claims = await _userService.QueryUserClaims(userInfo).ConfigureAwait(false);

            // 根据调用环境获取需要的权限
            if (VerifyTextCommandPermission(claims, message.Chat.Type, cmd))
            {
                try
                {
                    await CallQueryCommandAsync(userInfo, query, message, definition.ClassType, definition.Method, args).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    errorMsg = $"{ex.GetType} {ex.Message}";
                    _logger.LogError(ex, "回调命令 {cmd} 执行出错", cmd);
                    await _botClient.AutoReply(_optionsSetting.System.Debug ? errorMsg : "遇到内部错误", query, true).ConfigureAwait(false);
                }
                handled = true;
            }
            else
            {
                await _botClient.SendCommandReply("没有权限这么做", message).ConfigureAwait(false);
            }
        }
        else
        {
            await _botClient.SendCommandReply("未知的命令", message).ConfigureAwait(false);
        }

        //await _cmdRecordService.AddCmdRecord(query, dbUser, handled, true, errorMsg).ConfigureAwait(false);
    }

    /// <summary>
    /// 调用特定命令
    /// </summary>
    /// <param name="userInfo"></param>
    /// <param name="query"></param>
    /// <param name="message"></param>
    /// <param name="type"></param>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private async Task CallQueryCommandAsync(UserInfo userInfo, CallbackQuery query, Message message, Type type, MethodInfo method, string[] args)
    {
        //获取服务
        var service = _serviceScope.ServiceProvider.GetRequiredService(type);
        var methodParameters = new List<object>();
        //组装函数的入参
        foreach (var parameter in method.GetParameters())
        {
            var paramType = parameter.ParameterType;

            if (paramType == typeof(UserInfo))
            {
                methodParameters.Add(userInfo);
            }
            else if (paramType == typeof(CallbackQuery))
            {
                methodParameters.Add(query);
            }
            else if (paramType == typeof(Message))
            {
                methodParameters.Add(message);
            }
            else if (paramType == typeof(string[]))
            {
                var methodArgs = args.Length > 0 ? args[1..] : [];
                methodParameters.Add(methodArgs);
            }
            else if (paramType == typeof(List<string>))
            {
                var methodArgs = args.Length > 0 ? args[1..] : [];
                methodParameters.Add(methodArgs.ToList());
            }
            else
            {
                _logger.LogError("无效的参数类型 {paramName}", paramType.FullName);
            }
        }

        //调用方法
        if (method.Invoke(service, [.. methodParameters]) is Task task)
        {
            await task.ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task<List<BotCommand>> GetAvailabeCommands(UserInfo userInfo, ChatType chatType)
    {
        var claims = await _userService.QueryUserClaims(userInfo).ConfigureAwait(false);

        List<BotCommand> commands = [];

        foreach (var (command, definition) in _textCommandDefinitions)
        {
            if (VerifyTextCommandPermission(claims, chatType, command))
            {
                var botCommand = new BotCommand {
                    Command = command,
                    Description = definition.Attribute?.Description ?? "",
                };

                commands.Add(botCommand);
            }
        }

        return commands;
    }

    /// <inheritdoc />
    public async Task<bool> SetCommandsMenu()
    {
        //var privateCommands = GetAvailabeCommands(;
        //var privateCommands = new List<BotCommand>();
        //var privateCommands = new List<BotCommand>();

        //AddCommands(EUserRights.None);
        //AddCommands(EUserRights.NormalCmd);
        //await _botClient.SetMyCommands(commands, null).ConfigureAwait(false);
        //await _botClient.SetMyCommands(commands, new BotCommandScopeAllPrivateChats()).ConfigureAwait(false);
        //await _botClient.SetMyCommands(commands, new BotCommandScopeAllGroupChats()).ConfigureAwait(false);

        //AddCommands(EUserRights.AdminCmd);
        //await _botClient.SetMyCommands(commands, new BotCommandScopeAllChatAdministrators()).ConfigureAwait(false);

        //AddCommands(EUserRights.ReviewPost);
        //await _botClient.SetMyCommands(cmds, new BotCommandScopeChatAdministrators { ChatId = _channelService.ReviewGroup.Id }).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ClearCommandsMenu()
    {
        List<BotCommand> commands = [];
        string? languageCode = null;

        await _botClient.SetMyCommands(commands, null, languageCode).ConfigureAwait(false);
        await _botClient.SetMyCommands(commands, new BotCommandScopeAllPrivateChats(), languageCode).ConfigureAwait(false);
        await _botClient.SetMyCommands(commands, new BotCommandScopeAllGroupChats(), languageCode).ConfigureAwait(false);
        await _botClient.SetMyCommands(commands, new BotCommandScopeAllChatAdministrators(), languageCode).ConfigureAwait(false);
        return true;
    }
}