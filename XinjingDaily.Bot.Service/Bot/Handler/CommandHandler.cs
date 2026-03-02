using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Infrastructure.Enums;
using XinjingDaily.Bot.Infrastructure.Model;
using XinjingDaily.Bot.Interface.Bot;
using XinjingDaily.Bot.Interface.Common;

namespace XinjingDaily.Bot.Service.Bot.Handler;

[RegisterSingleton]
public class CommandHandler(
    ILogger<CommandHandler> _logger,
    IServiceProvider _serviceProvider,
    ITelegramBotService _botClient,
    IGlobalInfoService _globalInfo,
    IOptions<AppSettings> _options)
{
    private readonly AppSettings _optionsSetting = _options.Value;
    private readonly IServiceScope _serviceScope = _serviceProvider.CreateScope();

    /// <summary>
    /// 指令方法名映射
    /// </summary>
    private readonly Dictionary<Type, Dictionary<string, AssemblyMethod>> _commandClass = [];
    /// <summary>
    /// 指令别名
    /// </summary>
    private readonly Dictionary<Type, Dictionary<string, string>> _commandAlias = [];

    /// <summary>
    /// Query指令方法名映射
    /// </summary>
    private readonly Dictionary<Type, Dictionary<string, AssemblyMethod>> _queryCommandClass = [];
    /// <summary>
    /// Query指令别名
    /// </summary>
    private readonly Dictionary<Type, Dictionary<string, string>> _queryCommandAlias = [];


    public void RegisterTextCommand()
    {

    }

    public void RegisterQueryCommand()
    {

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
        foreach (var type in _commandClass.Keys)
        {
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
                    await _botClient.SendChatAction(message, ChatAction.Typing).ConfigureAwait(false);

                    await CallCommandAsync(userInfo, message, type, method).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    errorMsg = $"{ex.GetType} {ex.Message}";
                    _logger.LogError(ex, "命令 {cmd} 执行出错", cmd);
                    await _botClient.SendCommandReply(_optionsSetting.System.Debug ? errorMsg : "遇到内部错误", message).ConfigureAwait(false);
                }
                handled = true;
                break;
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
    private async Task CallCommandAsync(UserInfo userInfo, Message message, Type type, AssemblyMethod assemblyMethod)
    {
        //权限检查
        //if (!userInfo.Right.HasFlag(assemblyMethod.Rights))
        //{
        //    await _botClient.SendCommandReply("没有权限这么做", message).ConfigureAwait(false);
        //    return;
        //}

        //获取服务
        var service = _serviceScope.ServiceProvider.GetRequiredService(type);
        var method = assemblyMethod.Method;
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

        bool handled = false;
        string? errorMsg = null;
        //寻找注册的命令处理器
        foreach (var type in _queryCommandClass.Keys)
        {
            var allAlias = _queryCommandAlias[type];
            if (allAlias.TryGetValue(cmd, out var alias))
            {
                cmd = alias;
            }

            var allMethods = _queryCommandClass[type];
            if (allMethods.TryGetValue(cmd, out var method))
            {
                try
                {
                    await CallQueryCommandAsync(dbUser, query, type, method, args).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    errorMsg = $"{ex.GetType} {ex.Message}";
                    _logger.LogError(ex, "回调命令 {cmd} 执行出错", cmd);
                    await _botClient.AutoReply(_optionsSetting.System.Debug ? errorMsg : "遇到内部错误", query, true).ConfigureAwait(false);
                }
                handled = true;
                break;
            }
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
    private async Task CallQueryCommandAsync(UserInfo userInfo, CallbackQuery query, Type type, AssemblyMethod assemblyMethod, string[] args)
    {
        //权限检查
        //if (!userInfo.Right.HasFlag(assemblyMethod.Rights))
        //{
        //    await _botClient.AutoReply("没有权限这么做", query, true).ConfigureAwait(false);
        //    return;
        //}

        //获取服务
        var service = _serviceScope.ServiceProvider.GetRequiredService(type);
        var method = assemblyMethod.Method;
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

        foreach (var type in _commandClass.Keys)
        {
            var allMethods = _commandClass[type];
            foreach (var cmd in allMethods.Keys)
            {
                var method = allMethods[cmd];

                //if (dbUser.Right.HasFlag(method.Rights))
                //{
                //    if (!string.IsNullOrEmpty(method.Description))
                //    {
                //        if (!cmds.TryAdd(cmd.ToLowerInvariant(), method.Description))
                //        {
                //            _logger.LogWarning("命令 {cmd} 重复, 请检查代码逻辑", cmd);
                //        }
                //    }
                //}
            }
        }

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
            foreach (var type in _commandClass.Keys)
            {
                var allMethods = _commandClass[type];
                foreach (var cmd in allMethods.Keys)
                {
                    var method = allMethods[cmd];
                    if (method.Rights == right)
                    {
                        if (!string.IsNullOrEmpty(method.Description))
                        {
                            cmds.Add(new BotCommand { Command = cmd.ToLowerInvariant(), Description = method.Description });
                        }
                    }
                }
            }
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