using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Infrastructure.Attribute;
using XinjingDaily.Bot.Infrastructure.Model;
using XinjingDaily.Bot.Interface.Bot;
using XinjingDaily.Bot.Interface.Common;
using XinjingDaily.Bot.Interface.InitService;
using XinjingDaily.Bot.IRepository.Channel;

namespace XinjingDaily.Bot.Service.InitService;

/// <summary>
/// 机器人初始化服务
/// </summary>
/// <param name="_logger"></param>
public class CommandInitializer(
    ILogger<BotInitializer> _logger,
    IOptions<AppSettings> _options,
    ITelegramBotService _botClient,
    IChannelInfoRepository _channelInfoRepository,
    IGlobalInfoService _globalInfo) : IInitializer
{
    /// <inheritdoc/>
    public int Order => 3;

    /// <inheritdoc/>
    public string Name => nameof(CommandInitializer);

    private static readonly char[] separator = [','];

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {


        // 从数据库获取每个Channel的信息
        var channelInfos = await _channelInfoRepository.GetAllAsync();
        _logger.LogInformation("获取到 {Count} 个频道信息", channelInfos.Count);

        //// 遍历每个频道信息，获取详细的chatInfo
        //foreach (var channelInfo in channelInfos)
        //{
        //    try
        //    {
        //        var chat = await _botClient.GetChatAsync(channelInfo.TelegramId);
        //        _logger.LogInformation("获取频道详细信息: {Title} (@{Username})", chat.Title, chat.Username);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "获取频道 {TelegramId} 信息失败", channelInfo.TelegramId);
        //    }
        //}
    }


    /// <inheritdoc />
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

    /// <inheritdoc />
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
                if (!commands.TryAdd(command, new AssemblyMethod(method, description, rights)))
                {
                    _logger.LogWarning("注册命令 {cmd} 失败, 命令名称重复", command);
                }

                //添加别名
                if (!string.IsNullOrEmpty(alias))
                {
                    var splitedAlias = alias.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var split in splitedAlias)
                    {
                        if (!commandAlias.TryAdd(split, command))
                        {
                            _logger.LogWarning("注册命令 {cmd} 别名 {alias} 失败, 命令别名重复", command, split);
                        }
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
                if (!queryCommands.TryAdd(command, new AssemblyMethod(method, description, rights)))
                {
                    _logger.LogWarning("注册Query命令 {cmd} 失败, 命令名称重复", command);
                }

                //添加别名
                if (!string.IsNullOrEmpty(alias))
                {
                    var splitedAlias = alias.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var split in splitedAlias)
                    {
                        if (!queryAlias.TryAdd(split, command))
                        {
                            _logger.LogWarning("注册Query命令 {cmd} 别名 {split} 失败, 命令别名重复", command, split);
                        }
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
}