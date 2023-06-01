using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Infrastructure.Model;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler
{

    /// <summary>
    /// 命令处理器
    /// </summary>
    [AppService(typeof(ICommandHandler), LifeTime.Singleton)]
    internal class CommandHandler : ICommandHandler
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

        [RequiresUnreferencedCode("不兼容剪裁")]
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
        [RequiresUnreferencedCode("不兼容剪裁")]
        private void RegisterCommands([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type)
        {
            var commands = new Dictionary<string, AssemblyMethod>();
            var commandAlias = new Dictionary<string, string>();
            var queryCommands = new Dictionary<string, AssemblyMethod>();
            var queryAlias = new Dictionary<string, string>();

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
                    var description = queryAttribute.Description;
                    var rights = queryAttribute.Rights;
                    queryCommands.Add(command, new(method, description, rights));

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

        public async Task OnCommandReceived(Users dbUser, Message message)
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

                        if (_channelService.IsGroupMessage(message.Chat.Id))
                        {
                            //删除原消息
                            _ = Task.Run(async () => {
                                await Task.Delay(TimeSpan.FromSeconds(30));
                                try
                                {
                                    await _botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                }
                                catch
                                {
                                    _logger.LogError("删除消息 {messageId} 失败", message.MessageId);
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMsg = $"{ex.GetType} {ex.Message}";
                        _logger.LogError(ex, "命令 {cmd} 执行出错", cmd);
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
            var methodParameters = new List<object>();
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
            if (method.Invoke(service, methodParameters.ToArray()) is Task task)
            {
                await task;
            }
        }

        public async Task OnQueryCommandReceived(Users dbUser, CallbackQuery query)
        {
            Message? message = query.Message;
            if (message == null)
            {
                await _botClient.AutoReplyAsync("消息不存在", query, true);
                return;
            }

            if (string.IsNullOrEmpty(query.Data))
            {
                await _botClient.RemoveMessageReplyMarkupAsync(message);
                return;
            }

            //切分命令参数
            string[] args = query.Data!.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
            string cmd = args.First().ToUpperInvariant();

            if (cmd == "CMD")
            {
                if (args.Length < 2 || !long.TryParse(args[1], out long userID))
                {
                    await _botClient.AutoReplyAsync("Payload 非法", query, true);
                    await _botClient.RemoveMessageReplyMarkupAsync(message);
                    return;
                }

                //判断消息发起人是不是同一个
                if (dbUser.UserID != userID)
                {
                    await _botClient.AutoReplyAsync("这不是你的消息, 请不要瞎点", query, true);
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
                        await CallQueryCommandAsync(dbUser, query, type, method, args);
                    }
                    catch (Exception ex)
                    {
                        errorMsg = $"{ex.GetType} {ex.Message}";
                        _logger.LogError(ex, "回调命令 {cmd} 执行出错", cmd);
                        await _botClient.AutoReplyAsync(_optionsSetting.Debug ? errorMsg : "遇到内部错误", query, true);
                    }
                    handled = true;
                    break;
                }
            }

            await _cmdRecordService.AddCmdRecord(query, dbUser, handled, true, errorMsg);

            if (!handled)
            {
                if (_optionsSetting.Debug)
                {
                    await _botClient.AutoReplyAsync($"未知的命令 [{query.Data}]", query, true);
                }
                else
                {
                    await _botClient.AutoReplyAsync("未知的命令", query, true);
                }
            }
        }

        /// <summary>
        /// 调用特定命令
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="query"></param>
        /// <param name="type"></param>
        /// <param name="assemblyMethod"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task CallQueryCommandAsync(Users dbUser, CallbackQuery query, Type type, AssemblyMethod assemblyMethod, string[] args)
        {
            //权限检查
            if (!dbUser.Right.HasFlag(assemblyMethod.Rights))
            {
                await _botClient.AutoReplyAsync("没有权限这么做", query, true);
                return;
            }

            //获取服务
            var service = _serviceScope.ServiceProvider.GetRequiredService(type);
            var method = assemblyMethod.Method;
            var methodParameters = new List<object>();
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
                        methodParameters.Add(args);
                        break;

                    default:
                        _logger.LogDebug("{paramName}", parameter.ParameterType.Name);
                        break;
                }
            }
            //调用方法
            if (method.Invoke(service, methodParameters.ToArray()) is Task task)
            {
                await task;
            }
        }

        public string GetAvilabeCommands(Users dbUser)
        {
            var cmds = new Dictionary<string, string>();

            foreach (var type in _commandClass.Keys)
            {
                var allMethods = _commandClass[type];
                foreach (var cmd in allMethods.Keys)
                {
                    var method = allMethods[cmd];

                    if (dbUser.Right.HasFlag(method.Rights))
                    {
                        if (!string.IsNullOrEmpty(method.Description))
                        {
                            cmds.Add(cmd.ToLowerInvariant(), method.Description);
                        }
                    }
                }
            }

            if (cmds.Count > 0)
            {
                return string.Join('\n', cmds.OrderBy(x => x.Key).Select(x => $"/{x.Key} - {x.Value}"));
            }
            else
            {
                return "没有可用命令";
            }
        }

        public async Task<bool> GetCommandsMenu()
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
            await _botClient.SetMyCommandsAsync(cmds, new BotCommandScopeAllPrivateChats());
            await _botClient.SetMyCommandsAsync(cmds, new BotCommandScopeAllGroupChats());

            AddCommands(EUserRights.AdminCmd);
            await _botClient.SetMyCommandsAsync(cmds, new BotCommandScopeAllChatAdministrators());

            AddCommands(EUserRights.ReviewPost);
            await _botClient.SetMyCommandsAsync(cmds, new BotCommandScopeChatAdministrators { ChatId = _channelService.ReviewGroup.Id });
            return true;
        }
    }
}
