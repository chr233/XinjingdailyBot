using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository.Repositorys;
using XinjingdailyBot.Repository.Services;
using XinjingdailyBot.Service.Bot;
using XinjingdailyBot.Service.Common;

namespace XinjingdailyBot.Service.HostedService;

public sealed class BotInitializationServices(
    ILogger<BotInitializationServices> _logger,
    UpdateHandler _updateHandler,
    IHttpClientFactory _httpClientFactory,
    BotManagerService _botManagerService,
    BotRepository _botRepository,
    GroupRepository _groupRepository,
    LevelRepository _levelRepository,
    TagRepository _tagRepository,
    RejectReasonRepository _rejectReasonRepository
   ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("机器人初始化中");

        var bots = await _botRepository.QueryBotsEnabled().ConfigureAwait(false);

        var receiverOptions = new ReceiverOptions() {
            AllowedUpdates = [],
            DropPendingUpdates = false,
        };

        var tasks = bots.Select(bot => CreateAndValidBot(bot, receiverOptions, cancellationToken));
        await Task.WhenAll(tasks).ConfigureAwait(false);

        _logger.LogInformation("机器人初始化完成");


    }

    private async Task LoadSettingsFromDb()
    {
        _logger.LogInformation("读取基础信息");
        //await _channelService.InitChannelInfo().ConfigureAwait(false);

        _logger.LogInformation("读取群组和等级设定");
        await _groupRepository.InitGroupCache().ConfigureAwait(false);
        await _levelRepository.InitLevelCache().ConfigureAwait(false);
        await _tagRepository.InitPostTagCache().ConfigureAwait(false);
        await _rejectReasonRepository.InitRejectReasonCache().ConfigureAwait(false);
    }


    private async Task CreateAndValidBot(Bots bot, ReceiverOptions receiverOptions, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("Telegram");
        var telegramOption = new TelegramBotClientOptions(bot.BotToken);
        var botClient = new TelegramBotClient(telegramOption, httpClient, cancellationToken);

        try
        {
            var me = await botClient.GetMeAsync(cancellationToken).ConfigureAwait(false);

            //更新数据库
            if (bot.UserId != me.Id || bot.Username != me.Username || bot.Firstname != me.FirstName)
            {
                bot.UserId = me.Id;
                bot.Username = me.Username;
                bot.Firstname = me.FirstName;
                await _botRepository.UpdateBot(bot).ConfigureAwait(false);
            }

            _logger.LogInformation("机器人 {Id} BotId {botId} 登陆成功, 用户名 @{username} 昵称 {nickname}", bot.Id, me.Id, me.Username, me.FirstName);

            _botManagerService.AddBot(bot.Id, botClient);

            await botClient.ReceiveAsync(_updateHandler, receiverOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "机器人 {Id} BotId {botId} 登陆失败, 请检查 Token 是否有效以及网络配置是否正确", bot.Id, botClient.BotId);
        }
    }
}
