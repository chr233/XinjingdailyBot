using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Infrastructure.Extensions;
using XinjingDaily.Bot.Interface.Bot;
using XinjingDaily.Bot.Interface.Common;
using XinjingDaily.Bot.Interface.InitService;
using XinjingDaily.Bot.IRepository.Channel;

namespace XinjingDaily.Bot.Service.InitService;

/// <summary>
/// 机器人初始化服务
/// </summary>
/// <param name="_logger"></param>
[RegisterScoped<IServiceInitializer>(Duplicate = DuplicateStrategy.Append)]
public class BotInitializer(
    ILogger<BotInitializer> _logger,
    IOptions<AppSettings> _options,
    ITelegramBotService _botClient,
    IChannelInfoRepository _channelInfoRepository,
    IGlobalInfoService _globalInfo) : IServiceInitializer
{
    /// <inheritdoc/>
    public int Order => 10;

    /// <inheritdoc/>
    public string Name => nameof(BotInitializer);

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        try
        {
            var me = await _botClient.GetMe().ConfigureAwait(false);
            _globalInfo.BotUser = me;
            _logger.LogInformation("机器人信息获取成功: {BotName} (@{BotUsername})", me.FullName(), me.Username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取机器人信息失败, 请检查 Bot.BotToken 和 Bot.BotProxy");
            throw;
        }




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
}