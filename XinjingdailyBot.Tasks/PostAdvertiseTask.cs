using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;

namespace XinjingdailyBot.Tasks;

/// <summary>
/// 发布广告
/// </summary>
[Job("0 0 9 * * ?")]
internal class PostAdvertiseTask : IJob
{
    private readonly ILogger<PostAdvertiseTask> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITelegramBotClient _botClient;
    private readonly IAdvertisesService _advertisesService;

    public PostAdvertiseTask(
        ILogger<PostAdvertiseTask> logger,
        IServiceProvider serviceProvider,
        ITelegramBotClient botClient,
        IAdvertisesService advertisesService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _botClient = botClient;
        _advertisesService = advertisesService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("开始定时任务, 发布广告");

        var ad = await _advertisesService.GetPostableAdvertise();

        if (ad == null)
        {
            return;
        }

        var channelService = _serviceProvider.GetService<IChannelService>();

        if (channelService == null)
        {
            _logger.LogError("获取服务 {type} 失败", nameof(IChannelService));
            return;
        }

        var operates = new List<(EAdMode, ChatId)>
        {
           new (EAdMode.AcceptChannel, channelService.AcceptChannel.Id),
           new (EAdMode.RejectChannel, channelService.RejectChannel.Id),
           new (EAdMode.ReviewGroup, channelService.ReviewGroup.Id),
           new (EAdMode.CommentGroup, channelService.CommentGroup.Id),
           new (EAdMode.SubGroup, channelService.SubGroup.Id),
        };

        foreach (var (mode, chatId) in operates)
        {
            if (ad.Mode.HasFlag(mode) && chatId.Identifier != 0)
            {
                try
                {
                    var msgId = await _botClient.CopyMessageAsync(chatId, ad.ChatID, (int)ad.MessageID, disableNotification: true);
                    ad.ShowCount++;
                    ad.LastPostAt = DateTime.Now;
                    if (ad.PinMessage)
                    {
                        await _botClient.PinChatMessageAsync(chatId, msgId.Id, true);
                    }
                    await _advertisesService.Updateable(ad).UpdateColumns(x => new { x.ShowCount, x.LastPostAt }).ExecuteCommandAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "投放广告出错");
                }
                finally
                {
                    await Task.Delay(500);
                }
            }
        }
    }
}
