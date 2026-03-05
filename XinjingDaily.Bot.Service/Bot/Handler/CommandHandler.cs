using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingDaily.Bot.Data.Bot;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Infrastructure.Attribute;
using XinjingDaily.Bot.Infrastructure.Enums;
using XinjingDaily.Bot.Interface.Bot;
using XinjingDaily.Bot.Interface.Bot.Handler;
using XinjingDaily.Bot.Interface.Common;

namespace XinjingDaily.Bot.Service.Bot.Handler;

[RegisterSingleton(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class CommandHandler(
    ILogger<CommandHandler> _logger,
    IServiceProvider _serviceProvider,
    ITelegramBotService _botClient,
    IGlobalInfoService _globalInfo,
    IOptions<AppSettings> _options) : ICommandHandler
{
    private readonly AppSettings _optionsSetting = _options.Value;
    private readonly IServiceScope _serviceScope = _serviceProvider.CreateScope();

    private readonly Dictionary<(ECommandScope scope, string command), string?> _textCommandPermission = [];

    private readonly Dictionary<string, CommandDefinition<TextCommandAttribute>> _textCommandDefinitions = [];

    private readonly Dictionary<(ECommandScope scope, string command), string?> _queryCommandPermission = [];

    private readonly Dictionary<string, CommandDefinition<QueryCommandAttribute>> _queryCommandDefinitions = [];

    private static string[] SplitAlias(string alias)
    {
        return alias.ToUpperInvariant().Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public void RegisterTextCommand(Type classType, MethodInfo methodInfo, TextCommandAttribute attribute)
    {
        RegisterTextCommand(classType, methodInfo, ECommandScope.All, null, attribute);
    }

    public void RegisterTextCommand(Type classType, MethodInfo methodInfo, PermissionAttribute permission, TextCommandAttribute attribute)
    {
        RegisterTextCommand(classType, methodInfo, permission.Scope, permission.Permission, attribute);
    }

    public void RegisterTextCommand(Type classType, MethodInfo methodInfo, ECommandScope scope, string? permission, TextCommandAttribute attribute)
    {
        var command = attribute.Command.ToUpperInvariant();
        permission = permission?.ToUpperInvariant();
        var definition = new CommandDefinition<TextCommandAttribute>(classType, methodInfo, attribute);

        bool isDefinitionAdded = _textCommandDefinitions.TryAdd(command, definition);
        bool isPermissionAdded = _textCommandPermission.TryAdd((scope, command), permission);

        if (!isDefinitionAdded || !isPermissionAdded)
        {
            _logger.LogWarning("命令 {scope} - {command} - {permission} 已经存在, 请检查代码逻辑", scope, command, permission);
        }
        else
        {
            _logger.LogTrace("注册 {scope} - {command} - {permission}", scope, command, permission);
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

        if (!_queryCommandDefinitions.TryAdd(command, definition))
        {
            _logger.LogWarning("命令 {scope} - {command} - {permission} 已经存在, 请检查代码逻辑", scope, command, permission);
        }
        _queryCommandPermission.TryAdd((scope, command), permission);

        if (!string.IsNullOrEmpty(attribute.Alias))
        {
            var entries = SplitAlias(attribute.Alias);
            foreach (var entry in entries)
            {
                if (!_queryCommandDefinitions.TryAdd(command, definition))
                {
                    _logger.LogWarning("命令 {command} 的别名 {alias} 已经存在, 请检查代码逻辑", command, entry);
                }
                _queryCommandPermission.TryAdd((scope, entry), permission);
            }
        }
    }

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

    /// <inheritdoc />
    public async Task OnCommandReceived(UserInfo userInfo, Message message)
    {
        if (string.IsNullOrEmpty(message.Text))
        {
            return;
        }

        //切分命令参数
        string[] args = message.Text!.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
        string cmd = args.First()[1..].ToUpperInvariant();
        bool inGroup = message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup;

        //判断是不是艾特机器人的命令
        bool IsAtMe = false;
        int index = cmd.IndexOf('@');
        if (inGroup && index > -1)
        {
            string botName = cmd[(index + 1)..];
            if (botName.Equals(_globalInfo.BotUser.Username, StringComparison.OrdinalIgnoreCase))
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
        if (_textCommandDefinitions.TryGetValue(cmd, out var definition))
        {
            // 根据调用环境获取需要的权限
            if (VerifyTextCommandPermission(userInfo.Claims, message.Chat.Type, cmd))
            {
                try
                {
                    await _botClient.SendChatAction(message, ChatAction.Typing).ConfigureAwait(false);

                    await CallCommandAsync(userInfo, message, definition.ClassType, definition.Method).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    errorMsg = $"{ex.GetType} {ex.Message}";
                    _logger.LogError(ex, "命令 {cmd} 执行出错", cmd);
                    await _botClient.SendCommandReply(_optionsSetting.System.Debug ? errorMsg : "遇到内部错误", message).ConfigureAwait(false);
                }
                handled = true;
            }
        }

        //await _cmdRecordService.AddCmdRecord(message, userInfo, handled, false, errorMsg).ConfigureAwait(false);

        if (!handled && ((inGroup && IsAtMe) || (!inGroup)))
        {
            await _botClient.SendCommandReply("未知的命令", message).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 调用特定命令
    /// </summary>
    /// <param name="userInfo"></param>
    /// <param name="message"></param>
    /// <param name="type"></param>
    /// <param name="assemblyMethod"></param>
    /// <returns></returns>
    private async Task CallCommandAsync(UserInfo userInfo, Message message, Type type, MethodInfo method)
    {
        //权限检查
        //if (!userInfo.Right.HasFlag(assemblyMethod.Rights))
        //{
        //    await _botClient.SendCommandReply("没有权限这么做", message).ConfigureAwait(false);
        //    return;
        //}

        //获取服务
        var service = _serviceScope.ServiceProvider.GetRequiredService(type);
        var methodParameters = new List<object>();
        //组装函数的入参
        foreach (var parameter in method.GetParameters())
        {
            switch (parameter.ParameterType.Name)
            {
                case nameof(UserInfo):
                    methodParameters.Add(userInfo);
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
        if (method.Invoke(service, [.. methodParameters]) is Task task)
        {
            await task.ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task OnQueryCommandReceived(UserInfo dbUser, CallbackQuery query)
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
        string[] args = query.Data!.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
        string cmd = args.First().ToUpperInvariant();

        if (cmd == "CMD")
        {
            if (args.Length < 2 || !long.TryParse(args[1], out long userID))
            {
                await _botClient.AutoReply("Payload 非法", query, true).ConfigureAwait(false);
                await _botClient.RemoveMessageReplyMarkup(message).ConfigureAwait(false);
                return;
            }

            //判断消息发起人是不是同一个, userID 为 -1 时所有人均可用
            if (dbUser.TelegramId != userID && userID != -1)
            {
                await _botClient.AutoReply("这不是你的消息, 请不要瞎点", query, true).ConfigureAwait(false);
                return;
            }

            args = args[2..];
            cmd = args.First().ToUpperInvariant();
        }

        // 根据调用环境获取需要的权限
        var chatType = message.Chat.Type;
        string? permission;
        if (chatType == ChatType.Group || chatType == ChatType.Supergroup)
        {
            permission = _textCommandPermission.GetValueOrDefault((ECommandScope.Group, cmd), null)
                ?? _textCommandPermission.GetValueOrDefault((ECommandScope.All, cmd), null);
        }
        else if (chatType == ChatType.Private)
        {
            permission = _textCommandPermission.GetValueOrDefault((ECommandScope.Private, cmd), null)
                ?? _textCommandPermission.GetValueOrDefault((ECommandScope.All, cmd), null);
        }
        else
        {
            _logger.LogWarning("不支持在 ChatType = {type} 中调用命令", chatType);
            return;
        }

        //todo 权限验证


        bool handled = false;
        string? errorMsg = null;
        //寻找注册的命令处理器
        if (_queryCommandDefinitions.TryGetValue(cmd, out var definition))
        {
            try
            {
                await CallQueryCommandAsync(dbUser, query, definition.ClassType, definition.Method, args).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                errorMsg = $"{ex.GetType} {ex.Message}";
                _logger.LogError(ex, "回调命令 {cmd} 执行出错", cmd);
                await _botClient.AutoReply(_optionsSetting.System.Debug ? errorMsg : "遇到内部错误", query, true).ConfigureAwait(false);
            }
            handled = true;
        }

        //await _cmdRecordService.AddCmdRecord(query, dbUser, handled, true, errorMsg).ConfigureAwait(false);

        if (!handled)
        {
            if (_optionsSetting.System.Debug)
            {
                await _botClient.AutoReply($"未知的命令 [{query.Data}]", query, true).ConfigureAwait(false);
            }
            else
            {
                await _botClient.AutoReply("未知的命令", query, true).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// 调用特定命令
    /// </summary>
    /// <param name="userInfo"></param>
    /// <param name="query"></param>
    /// <param name="type"></param>
    /// <param name="assemblyMethod"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private async Task CallQueryCommandAsync(UserInfo userInfo, CallbackQuery query, Type type, MethodInfo method, string[] args)
    {
        //权限检查
        //if (!userInfo.Right.HasFlag(assemblyMethod.Rights))
        //{
        //    await _botClient.AutoReply("没有权限这么做", query, true).ConfigureAwait(false);
        //    return;
        //}

        //获取服务
        var service = _serviceScope.ServiceProvider.GetRequiredService(type);
        var methodParameters = new List<object>();
        //组装函数的入参
        foreach (var parameter in method.GetParameters())
        {
            switch (parameter.ParameterType.Name)
            {
                case nameof(UserInfo):
                    methodParameters.Add(userInfo);
                    break;
                case nameof(CallbackQuery):
                    methodParameters.Add(query);
                    break;
                case "String[]":
                    methodParameters.Add(args);
                    break;

                default:
                    _logger.LogDebug("{paramName}", parameter.ParameterType.Name);
                    break;
            }
        }
        //调用方法
        if (method.Invoke(service, [.. methodParameters]) is Task task)
        {
            await task.ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public string GetAvailabeCommands(UserInfo dbUser)
    {
        var cmds = new Dictionary<string, string>();

        //foreach (var type in _commandClass.Keys)
        //{
        //    var allMethods = _commandClass[type];
        //    foreach (var cmd in allMethods.Keys)
        //    {
        //        var method = allMethods[cmd];

        //        //if (dbUser.Right.HasFlag(method.Rights))
        //        //{
        //        //    if (!string.IsNullOrEmpty(method.Description))
        //        //    {
        //        //        if (!cmds.TryAdd(cmd.ToLowerInvariant(), method.Description))
        //        //        {
        //        //            _logger.LogWarning("命令 {cmd} 重复, 请检查代码逻辑", cmd);
        //        //        }
        //        //    }
        //        //}
        //    }
        //}

        if (cmds.Count > 0)
        {
            var sb = new StringBuilder();
            foreach (var cmd in cmds.OrderBy(static x => x.Key))
            {
                sb.AppendLine($"/{cmd.Key} - {cmd.Value}");
            }
            return sb.ToString();
        }
        else
        {
            return "没有可用命令";
        }
    }

    /// <inheritdoc />
    public async Task<bool> SetCommandsMenu()
    {
        var cmds = new List<BotCommand>();

        void AddCommands(EUserRights right)
        {
            //foreach (var type in _commandClass.Keys)
            //{
            //    var allMethods = _commandClass[type];
            //    foreach (var cmd in allMethods.Keys)
            //    {
            //        var method = allMethods[cmd];
            //        if (method.Rights == right)
            //        {
            //            if (!string.IsNullOrEmpty(method.Description))
            //            {
            //                cmds.Add(new BotCommand { Command = cmd.ToLowerInvariant(), Description = method.Description });
            //            }
            //        }
            //    }
            //}
        }

        AddCommands(EUserRights.None);
        AddCommands(EUserRights.NormalCmd);
        await _botClient.SetMyCommands(cmds, null).ConfigureAwait(false);
        await _botClient.SetMyCommands(cmds, new BotCommandScopeAllPrivateChats()).ConfigureAwait(false);
        await _botClient.SetMyCommands(cmds, new BotCommandScopeAllGroupChats()).ConfigureAwait(false);

        AddCommands(EUserRights.AdminCmd);
        await _botClient.SetMyCommands(cmds, new BotCommandScopeAllChatAdministrators()).ConfigureAwait(false);

        AddCommands(EUserRights.ReviewPost);
        //await _botClient.SetMyCommands(cmds, new BotCommandScopeChatAdministrators { ChatId = _channelService.ReviewGroup.Id }).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ClearCommandsMenu()
    {
        var cmds = new List<BotCommand>();

        await _botClient.SetMyCommands(cmds).ConfigureAwait(false);
        await _botClient.SetMyCommands(cmds, new BotCommandScopeAllPrivateChats()).ConfigureAwait(false);
        await _botClient.SetMyCommands(cmds, new BotCommandScopeAllGroupChats()).ConfigureAwait(false);
        await _botClient.SetMyCommands(cmds, new BotCommandScopeAllChatAdministrators()).ConfigureAwait(false);
        //await _botClient.SetMyCommands(cmds, new BotCommandScopeChatAdministrators { ChatId = _channelService.ReviewGroup.Id }).ConfigureAwait(false);
        return true;
    }
}